using SistemaLevels.Models;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface ITiposContratosService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(TiposContrato model);
        Task<bool> Insertar(TiposContrato model);

        Task<TiposContrato> Obtener(int id);

        Task<IQueryable<TiposContrato>> ObtenerTodos();
    }

}
