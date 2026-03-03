using SistemaLevels.BLL.Common;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IUbicacionesService
    {
        Task<ServiceResult> Insertar(Ubicacion model);

        Task<ServiceResult> Actualizar(Ubicacion model);

        Task<ServiceResult> Eliminar(int id);

        Task<Ubicacion?> Obtener(int id);

        Task<IQueryable<Ubicacion>> ObtenerTodos();
    }
}