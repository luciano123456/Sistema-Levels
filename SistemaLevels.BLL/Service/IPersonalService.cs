using SistemaLevels.BLL.Common;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IPersonalService
    {
        Task<ServiceResult> Insertar(
            Personal model,
            List<int> rolesIds,
            List<int> artistasIds);

        Task<ServiceResult> Actualizar(
            Personal model,
            List<int> rolesIds,
            List<int> artistasIds);

        Task<ServiceResult> Eliminar(int id);

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
            int? idArtista);
    }
}