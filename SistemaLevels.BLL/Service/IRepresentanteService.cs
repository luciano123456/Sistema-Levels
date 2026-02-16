using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IRepresentantesService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Representante model);
        Task<bool> Insertar(Representante model);

        Task<Representante> Obtener(int id);
        Task<IQueryable<Representante>> ObtenerTodos();
    }
}
