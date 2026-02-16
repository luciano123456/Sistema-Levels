using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface ITareasService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Tarea model);
        Task<bool> Insertar(Tarea model);

        Task<Tarea?> Obtener(int id);
        Task<IQueryable<Tarea>> ObtenerTodos();
    }
}
