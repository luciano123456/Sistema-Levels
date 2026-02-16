using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IClientesService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Cliente model);
        Task<bool> Insertar(Cliente model);

        Task<Cliente?> Obtener(int id);
        Task<IQueryable<Cliente>> ObtenerTodos();
    }
}
