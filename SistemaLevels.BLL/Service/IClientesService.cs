using SistemaLevels.BLL.Common;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IClientesService
    {
        Task<ServiceResult> Insertar(
            Cliente model,
            List<int> productorasIds);

        Task<ServiceResult> Actualizar(
            Cliente model,
            List<int> productorasIds);

        Task<ServiceResult> Eliminar(int id);

        Task<Cliente?> Obtener(int id);

        Task<IQueryable<Cliente>> ObtenerTodos();
    }
}