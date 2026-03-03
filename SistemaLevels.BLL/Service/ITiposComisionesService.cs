using SistemaLevels.Models;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface ITiposComisionesService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(TiposComision model);
        Task<bool> Insertar(TiposComision model);

        Task<TiposComision> Obtener(int id);

        Task<IQueryable<TiposComision>> ObtenerTodos();
    }

}
