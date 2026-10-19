using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abtracciones.Modelos
{
    public class TicketDTO
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(100, ErrorMessage = "El título no puede ser mayor a 100 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La prioridad es obligatoria")]
        public PrioridadTicket Prioridad { get; set; }

        [Required(ErrorMessage = "El ClienteId es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ClienteId debe ser un numero entero")]
        public int ClienteId { get; set; }
    }
}
