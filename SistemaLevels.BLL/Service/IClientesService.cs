using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IClientesService
    {
        Task<bool> Insertar(Cliente model, List<int> productorasIds);

        Task<bool> Actualizar(Cliente model, List<int> productorasIds);

        Task<bool> Eliminar(int id);

        Task<Cliente?> Obtener(int id);

        Task<IQueryable<Cliente>> ObtenerTodos();
    }
}