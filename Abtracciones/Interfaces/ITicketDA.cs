using Abtracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abtracciones.Interfaces
{
    public interface ITicketDA
    {
        List<Ticket> ObtenerTodos();
        Ticket? ObtenerPorId(int id);
        Ticket? ObtenerPorCodigo(string codigo);
        int ObtenerSiguienteId();
        int ObtenerSiguienteSecuencia();
        void Agregar(Ticket ticket);
        void Actualizar(Ticket ticket);
    }
}
