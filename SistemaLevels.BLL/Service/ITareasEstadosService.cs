using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface ITareasEstadosService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(TareasEstado model);
        Task<bool> Insertar(TareasEstado model);

        Task<TareasEstado> Obtener(int id);

        Task<IQueryable<TareasEstado>> ObtenerTodos();
    }

}
