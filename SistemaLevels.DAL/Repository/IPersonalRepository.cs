using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public interface IPersonalRepository
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Personal model, List<int> rolesIds, List<int> artistasIds);
        Task<bool> Insertar(Personal model, List<int> rolesIds, List<int> artistasIds);

        Task<Personal?> Obtener(int id);
        Task<IQueryable<Personal>> ObtenerTodos();

        Task<List<int>> ObtenerRolesIds(int idPersonal);
        Task<List<int>> ObtenerArtistasIds(int idPersonal);

        Task<IQueryable<Personal>> ListarFiltrado(
           string? nombre,
           int? idPais,
           int? idTipoDocumento,
           int? idCondicionIva,
           int? idRol,
           int? idArtista
       );
    }
}
