using Microsoft.EntityFrameworkCore;
using SistemaLevels.BLL.Common;
using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class UbicacionesService : IUbicacionesService
    {
        private readonly IUbicacionesRepository<Ubicacion> _repo;

        public UbicacionesService(IUbicacionesRepository<Ubicacion> repo)
        {
            _repo = repo;
        }

        public async Task<ServiceResult> Insertar(Ubicacion model)
        {
            if (string.IsNullOrWhiteSpace(model.Descripcion))
                return ServiceResult.Error("Debe ingresar una descripción.", "validacion");

            var dup = await _repo.BuscarDuplicado(null, model.Descripcion);
            if (dup != null)
                return ServiceResult.Error($"Ya existe la ubicación '{dup.Descripcion}'.", "duplicado", dup.Id);

            var ok = await _repo.Insertar(model);

            return ok
                ? ServiceResult.Success("Ubicación registrada correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        public async Task<ServiceResult> Actualizar(Ubicacion model)
        {
            var dup = await _repo.BuscarDuplicado(model.Id, model.Descripcion);
            if (dup != null)
                return ServiceResult.Error($"Ya existe la ubicación '{dup.Descripcion}'.", "duplicado", dup.Id);

            var ok = await _repo.Actualizar(model);

            return ok
                ? ServiceResult.Success("Ubicación modificada correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        public async Task<ServiceResult> Eliminar(int id)
        {
            try
            {
                var ok = await _repo.Eliminar(id);

                if (!ok)
                    return ServiceResult.Error("No se encontró el registro.");

                return ServiceResult.Success("Ubicación eliminada correctamente");
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
                return ServiceResult.Error("Error inesperado.");
            }
        }

        public Task<Ubicacion?> Obtener(int id) => _repo.Obtener(id);

        public Task<IQueryable<Ubicacion>> ObtenerTodos() => _repo.ObtenerTodos();
    }
}