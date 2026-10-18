

using Abtracciones.Modelos;

namespace Abtracciones.Interfaces
{
    public interface IClienteDA
    {
        Task<List<Cliente>> Obtener();
        Task<Cliente> ObtenerPorId(int id);
        Task<Cliente> Agregar(Cliente cliente);
        //return true si el cliente se actualiza en base al id
        Task<bool> Actualizar(int id, Cliente datos);
        //return true si el cliente se elimina, de lo contrario retorna false
        Task<bool> Eliminar(int id);
    }
}
