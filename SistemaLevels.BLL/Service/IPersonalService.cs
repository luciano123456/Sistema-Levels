using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IPersonalService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Personal model, List<int> rolesIds, List<int> artistasIds);
        Task<bool> Insertar(Personal model, List<int> rolesIds, List<int> artistasIds);

        Task<Personal?> Obtener(int id);
        Task<IQueryable<Personal>> ObtenerTodos();

        Task<List<int>> ObtenerRolesIds(int idPersonal);
        Task<List<int>> ObtenerArtistasIds(int idPersonal);
    }
}
