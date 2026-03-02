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

        /* =====================================================
           INSERTAR
        ===================================================== */

        public async Task<ServiceResult> Insertar(
            Personal model,
            List<int> rolesIds,
            List<int> artistasIds)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                return ServiceResult.Error(
                    "Debe completar los campos obligatorios.",
                    "validacion");
            }

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

            var ok = await _repo.Insertar(
                model,
                rolesIds,
                artistasIds);

            return ok
                ? ServiceResult.Success("Personal registrado correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        /* =====================================================
           ACTUALIZAR
        ===================================================== */

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

            var ok = await _repo.Actualizar(
                model,
                rolesIds,
                artistasIds);

            return ok
                ? ServiceResult.Success("Personal modificado correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        /* =====================================================
           ELIMINAR
        ===================================================== */

        public async Task<ServiceResult> Eliminar(int id)
        {
            var ok = await _repo.Eliminar(id);

            return ok
                ? ServiceResult.Success("Personal eliminado correctamente")
                : ServiceResult.Error("No se encontró el registro.");
        }

        public Task<Personal?> Obtener(int id)
            => _repo.Obtener(id);

        public Task<IQueryable<Personal>> ObtenerTodos()
            => _repo.ObtenerTodos();

        public Task<List<int>> ObtenerRolesIds(int idPersonal)
            => _repo.ObtenerRolesIds(idPersonal);

        public Task<List<int>> ObtenerArtistasIds(int idPersonal)
            => _repo.ObtenerArtistasIds(idPersonal);
    }
}