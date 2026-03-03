using SistemaLevels.Models;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IVentasEstadosService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(VentasEstado model);
        Task<bool> Insertar(VentasEstado model);

        Task<VentasEstado> Obtener(int id);

        Task<IQueryable<VentasEstado>> ObtenerTodos();
    }

}
