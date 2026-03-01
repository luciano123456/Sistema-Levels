using Microsoft.EntityFrameworkCore;
using SistemaLevels.BLL.Common;
using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class PersonalService : IPersonalService
    {
        private readonly IPersonalRepository _repo;

        public PersonalService(IPersonalRepository repo)
        {
            _repo = repo;
        }

        public async Task<ServiceResult> Insertar(
            Personal model,
            List<int> rolesIds,
            List<int> artistasIds)
        {
            var dup = await _repo.BuscarDuplicado(
                null,
                model.Nombre,
                model.Dni,
                model.NumeroDocumento);

            if (dup != null)
            {
                return ServiceResult.Error(
                    $"El registro coincide con '{dup.Nombre}'.",
                    "duplicado",
                    dup.Id);
            }

            var ok = await _repo.Insertar(model, rolesIds, artistasIds);

            return ok
                ? ServiceResult.Success("Personal registrado correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        public async Task<ServiceResult> Actualizar(
            Personal model,
            List<int> rolesIds,
            List<int> artistasIds)
        {
            var dup = await _repo.BuscarDuplicado(
                model.Id,
                model.Nombre,
                model.Dni,
                model.NumeroDocumento);

            if (dup != null)
            {
                return ServiceResult.Error(
                    $"El registro coincide con '{dup.Nombre}'.",
                    "duplicado",
                    dup.Id);
            }

            var ok = await _repo.Actualizar(model, rolesIds, artistasIds);

            return ok
                ? ServiceResult.Success("Personal modificado correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        public async Task<ServiceResult> Eliminar(int id)
        {
            try
            {
                var ok = await _repo.Eliminar(id);

                if (!ok)
                    return ServiceResult.Error("No se encontró el registro.");

                return ServiceResult.Success("Personal eliminado correctamente");
            }
            catch (DbUpdateException)
            {
                return ServiceResult.Error(
                    "No se puede eliminar porque posee registros relacionados.",
                    "relacion",
                    id);
            }
            catch
            {
                return ServiceResult.Error("Error inesperado al eliminar.");
            }
        }

        public Task<Personal?> Obtener(int id)
            => _repo.Obtener(id);

        public Task<IQueryable<Personal>> ObtenerTodos()
            => _repo.ObtenerTodos();

        public Task<List<int>> ObtenerRolesIds(int idPersonal)
            => _repo.ObtenerRolesIds(idPersonal);

        public Task<List<int>> ObtenerArtistasIds(int idPersonal)
            => _repo.ObtenerArtistasIds(idPersonal);

        public Task<IQueryable<Personal>> ListarFiltrado(
            string? nombre,
            int? idPais,
            int? idTipoDocumento,
            int? idCondicionIva,
            int? idRol,
            int? idArtista)
            => _repo.ListarFiltrado(
                nombre,
                idPais,
                idTipoDocumento,
                idCondicionIva,
                idRol,
                idArtista);
    }
}