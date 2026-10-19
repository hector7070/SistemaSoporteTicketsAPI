using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abtracciones.Modelos
{
    public class TicketEstadoDTO
    {
        [Required(ErrorMessage = "El estado es obligatorio.")]
        public EstadoTicket Estado { get; set; }

        public int? AgenteId { get; set; }

        public PrioridadTicket? Prioridad { get; set; }
    }
}
