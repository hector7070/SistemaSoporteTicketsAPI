using Abtracciones.Excepciones;
using Abtracciones.Interfaces;
using Abtracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json", "application/problem+json")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketFlujo _ticketFlujo;

        public TicketsController(ITicketFlujo ticketFlujo)
        {
            _ticketFlujo = ticketFlujo;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ObtenerPaginados(
            [FromQuery] EstadoTicket? estado,
            [FromQuery] PrioridadTicket? prioridad,
            [FromQuery] int? clienteId,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? q,
            [FromQuery] string? orden = "fecha",
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1)
            {
                return Problem(detail: "Los parámetros 'pagina' y 'tamano' deben ser enteros mayores a 0.", statusCode: 400, title: "Bad Request");
            }

            var (items, total) = _ticketFlujo.ObtenerPaginados(estado, prioridad, clienteId, desde, hasta, q, orden, pagina, tamano);

            Response.Headers.Append("X-Total-Count", total.ToString());
            Response.Headers.Append("X-Page", pagina.ToString());
            Response.Headers.Append("X-Page-Size", items.Count.ToString());

            return Ok(items);
        }

        [HttpGet("{id:int:min(1)}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ObtenerPorId(int id)
        {
            var ticket = _ticketFlujo.ObtenerPorId(id);
            if (ticket == null)
            {
                return Problem(detail: $"No se encontró el ticket con ID {id}.", statusCode: 404, title: "Not Found");
            }

            string etagCalculado = CalcularETag(ticket);
            Response.Headers.ETag = etagCalculado;

            if (Request.Headers.TryGetValue("If-None-Match", out var clientETag) && clientETag == etagCalculado)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            return Ok(ticket);
        }

        [HttpGet("codigo/{codigo:codigoTicket}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ObtenerPorCodigo(string codigo)
        {
            var ticket = _ticketFlujo.ObtenerPorCodigo(codigo);
            if (ticket == null)
            {
                return Problem(detail: $"No se encontró ningún ticket con el código '{codigo}'.", statusCode: 404, title: "Not Found");
            }

            return Ok(ticket);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Crear([FromBody] TicketDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var nuevoTicket = _ticketFlujo.Crear(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoTicket.Id }, nuevoTicket);
        }

        [HttpPatch("{id:int}/estado")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult ActualizarEstado(int id, [FromBody] TicketEstadoDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var actualizado = _ticketFlujo.ActualizarEstado(id, dto);
                return Ok(actualizado);
            }
            catch (NotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: 404, title: "Not Found");
            }
            catch (ReglaNegocio ex)
            {
                return Problem(detail: ex.Message, statusCode: 409, title: "Conflict - Regla de Negocio");
            }
        }

        private static string CalcularETag(Ticket ticket)
        {
            var rawData = $"{ticket.Id}-{ticket.FechaActualizacion.Ticks}-{ticket.Estado}-{ticket.Prioridad}";
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return $"\"{Convert.ToBase64String(bytes)}\"";
        }
    }
}
