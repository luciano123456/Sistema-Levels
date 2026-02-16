using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public interface ITareasRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Tarea model);
        Task<bool> Insertar(Tarea model);

        Task<Tarea?> Obtener(int id);
        Task<IQueryable<Tarea>> ObtenerTodos();
    }
}
