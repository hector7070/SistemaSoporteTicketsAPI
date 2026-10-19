using Abtracciones.Excepciones;
using Abtracciones.Interfaces;
using Abtracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class TicketFlujo : ITicketFlujo
    {
        private readonly ITicketDA _ticketDA;
        public TicketFlujo(ITicketDA ticketDA)
        {
            _ticketDA = ticketDA;
        }

        public (List<Ticket> Items, int TotalCount) ObtenerPaginados(
            EstadoTicket? estado,
            PrioridadTicket? prioridad,
            int? clienteId,
            DateTime? desde,
            DateTime? hasta,
            string? q,
            string? orden,
            int pagina,
            int tamano)
        {
            var consulta = _ticketDA.ObtenerTodos().AsQueryable();

            if (estado.HasValue) consulta = consulta.Where(t => t.Estado == estado.Value);
            if (prioridad.HasValue) consulta = consulta.Where(t => t.Prioridad == prioridad.Value);
            if (clienteId.HasValue) consulta = consulta.Where(t => t.ClienteId == clienteId.Value);
            if (desde.HasValue) consulta = consulta.Where(t => t.FechaCreacion >= desde.Value);
            if (hasta.HasValue) consulta = consulta.Where(t => t.FechaCreacion <= hasta.Value);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var termino = q.Trim().ToLower();
                consulta = consulta.Where(t => t.Titulo.ToLower().Contains(termino) || t.Descripcion.ToLower().Contains(termino));
            }

            consulta = orden?.ToLower() switch
            {
                "-fecha" => consulta.OrderByDescending(t => t.FechaCreacion),
                "prioridad" => consulta.OrderBy(t => t.Prioridad),
                "-prioridad" => consulta.OrderByDescending(t => t.Prioridad),
                _ => consulta.OrderBy(t => t.FechaCreacion) 
            };

            int total = consulta.Count();

            int tamanoAjustado = Math.Min(Math.Max(1, tamano), 25);
            int paginaAjustada = Math.Max(1, pagina);

            var items = consulta
                .Skip((paginaAjustada - 1) * tamanoAjustado)
                .Take(tamanoAjustado)
                .ToList();

            return (items, total);
        }

        public Ticket? ObtenerPorId(int id) => _ticketDA.ObtenerPorId(id);

        public Ticket? ObtenerPorCodigo(string codigo) => _ticketDA.ObtenerPorCodigo(codigo);

        public Ticket Crear(TicketDTO dto)
        {
            int siguienteId = _ticketDA.ObtenerSiguienteId();
            int siguienteSecuencia = _ticketDA.ObtenerSiguienteSecuencia();

            string codigoGenerado = $"INC-{siguienteSecuencia:D6}";

            var nuevoTicket = new Ticket
            {
                Id = siguienteId,
                Codigo = codigoGenerado,
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Prioridad = dto.Prioridad,
                Estado = EstadoTicket.Abierto,
                ClienteId = dto.ClienteId,
                AgenteId = null,
                Reaperturas = 0,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };

            _ticketDA.Agregar(nuevoTicket);
            return nuevoTicket;
        }

        public Ticket ActualizarEstado(int id, TicketEstadoDTO dto)
        {
            var ticket = _ticketDA.ObtenerPorId(id);
            if (ticket == null)
                throw new NotFoundException($"El ticket con ID {id} no existe.");

            ValidarTransicionEstado(ticket.Estado, dto.Estado);

            var nuevaPrioridad = dto.Prioridad ?? ticket.Prioridad;

            if (ticket.Estado == EstadoTicket.EnProceso && nuevaPrioridad == PrioridadTicket.Baja)
            {
                throw new ReglaNegocio("No se puede bajar la prioridad a Baja de un ticket que está EnProceso.");
            }

            ticket.Estado = dto.Estado;
            if (dto.AgenteId.HasValue) ticket.AgenteId = dto.AgenteId.Value;
            if (dto.Prioridad.HasValue) ticket.Prioridad = dto.Prioridad.Value;
            ticket.FechaActualizacion = DateTime.Now;

            _ticketDA.Actualizar(ticket);
            return ticket;
        }

        public Ticket Reabrir(int id)
        {
            var ticket = _ticketDA.ObtenerPorId(id);
            if (ticket == null)
                throw new NotFoundException($"El ticket con ID {id} no existe.");

            if (ticket.Estado != EstadoTicket.Resuelto)
                throw new ReglaNegocio("Solo se pueden reabrir tickets en estado Resuelto.");

            ticket.Estado = EstadoTicket.EnProceso;
            ticket.Reaperturas++;
            ticket.FechaActualizacion = DateTime.Now;

            _ticketDA.Actualizar(ticket);
            return ticket;
        }

        public object ObtenerResumen()
        {
            var todos = _ticketDA.ObtenerTodos();
            return new
            {
                total = todos.Count,
                porEstado = todos.GroupBy(t => t.Estado.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                porPrioridad = todos.GroupBy(t => t.Prioridad.ToString()).ToDictionary(g => g.Key, g => g.Count())
            };
        }

        private void ValidarTransicionEstado(EstadoTicket actual, EstadoTicket nuevo)
        {
            if (actual == nuevo) return;

            bool esValida = (actual, nuevo) switch
            {
                (EstadoTicket.Abierto, EstadoTicket.EnProceso) => true,
                (EstadoTicket.EnProceso, EstadoTicket.Resuelto) => true,
                (EstadoTicket.Resuelto, EstadoTicket.Cerrado) => true,
                (EstadoTicket.Resuelto, EstadoTicket.EnProceso) => true,
                _ => false
            };

            if (!esValida)
            {
                throw new ReglaNegocio($"No se permite la transición de estado de '{actual}' a '{nuevo}'.");
            }
        }
    }
}