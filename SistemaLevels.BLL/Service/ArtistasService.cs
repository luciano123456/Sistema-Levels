using Microsoft.EntityFrameworkCore;
using SistemaLevels.BLL.Common;
using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class ArtistasService : IArtistasService
    {
        private readonly IArtistasRepository<Artista> _repo;

        public ArtistasService(IArtistasRepository<Artista> repo)
        {
            _repo = repo;
        }

        /* ================= INSERTAR ================= */

        public async Task<ServiceResult> Insertar(Artista model)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre) ||
                string.IsNullOrWhiteSpace(model.NombreArtistico) ||
                string.IsNullOrWhiteSpace(model.NumeroDocumento) ||
                string.IsNullOrWhiteSpace(model.Dni))
            {
                return ServiceResult.Error(
                    "Debe completar los campos obligatorios.",
                    "validacion");
            }

            var dup = await _repo.BuscarDuplicado(
                null,
                model.Nombre,
                model.NombreArtistico,
                model.NumeroDocumento,
                model.Dni);

            if (dup != null)
            {
                return ServiceResult.Error(
                    $"Ya existe un artista: '{dup.NombreArtistico}'.",
                    "duplicado",
                    dup.Id);
            }

            var ok = await _repo.Insertar(model);

            return ok
                ? ServiceResult.Success("Artista registrado correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        /* ================= ACTUALIZAR ================= */

        public async Task<ServiceResult> Actualizar(Artista model)
        {
            var dup = await _repo.BuscarDuplicado(
                model.Id,
                model.Nombre,
                model.NombreArtistico,
                model.NumeroDocumento,
                model.Dni);

            if (dup != null)
            {
                return ServiceResult.Error(
                    $"Ya existe un artista: '{dup.NombreArtistico}'.",
                    "duplicado",
                    dup.Id);
            }

            var ok = await _repo.Actualizar(model);

            return ok
                ? ServiceResult.Success("Artista modificado correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        /* ================= ELIMINAR ================= */

        public async Task<ServiceResult> Eliminar(int id)
        {
            try
            {
                var ok = await _repo.Eliminar(id);

                if (!ok)
                    return ServiceResult.Error("No se encontró el registro.");

                return ServiceResult.Success("Artista eliminado correctamente");
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

        public Task<Artista?> Obtener(int id)
            => _repo.Obtener(id);

        public Task<IQueryable<Artista>> ObtenerTodos()
            => _repo.ObtenerTodos();
    }
}