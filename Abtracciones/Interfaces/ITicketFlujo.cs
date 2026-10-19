using Abtracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abtracciones.Interfaces
{
    public interface ITicketFlujo
    {
        (List<Ticket> Items, int TotalCount) ObtenerPaginados(
            EstadoTicket? estado,
            PrioridadTicket? prioridad,
            int? clienteId,
            DateTime? desde,
            DateTime? hasta,
            string? q,
            string? orden,
            int pagina,
            int tamano);

        Ticket? ObtenerPorId(int id);

        Ticket? ObtenerPorCodigo(string codigo);

        Ticket Crear(TicketDTO dto);

        Ticket ActualizarEstado(int id, TicketEstadoDTO dto);

        Ticket Reabrir(int id);

        object ObtenerResumen();

    }
}
