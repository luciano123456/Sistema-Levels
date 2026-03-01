using Microsoft.EntityFrameworkCore;
using SistemaLevels.BLL.Common;
using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class ProductorasService : IProductorasService
    {
        private readonly IProductorasRepository _repo;

        public ProductorasService(IProductorasRepository repo)
        {
            _repo = repo;
        }

        /* ================= INSERTAR ================= */

        public async Task<ServiceResult> Insertar(Productora model, List<int> clientesIds)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre) ||
                string.IsNullOrWhiteSpace(model.Dni) ||
                string.IsNullOrWhiteSpace(model.NumeroDocumento) ||
                string.IsNullOrWhiteSpace(model.Telefono) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                model.Idpais == null ||
                model.IdProvincia == null)
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
                    $"Ya existe una productora: '{dup.Nombre}'.",
                    "duplicado",
                    dup.Id);
            }

            var ok = await _repo.Insertar(model, clientesIds);

            return ok
                ? ServiceResult.Success("Productora registrada correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        /* ================= ACTUALIZAR ================= */

        public async Task<ServiceResult> Actualizar(Productora model, List<int> clientesIds)
        {
            var dup = await _repo.BuscarDuplicado(
                model.Id,
                model.Nombre,
                model.Dni,
                model.NumeroDocumento);

            if (dup != null)
            {
                return ServiceResult.Error(
                    $"Ya existe una productora: '{dup.Nombre}'.",
                    "duplicado",
                    dup.Id);
            }

            var ok = await _repo.Actualizar(model, clientesIds);

            return ok
                ? ServiceResult.Success("Productora modificada correctamente")
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

                return ServiceResult.Success("Productora eliminada correctamente");
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

        public Task<Productora?> Obtener(int id)
            => _repo.Obtener(id);

        public Task<IQueryable<Productora>> ObtenerTodos()
            => _repo.ObtenerTodos();

        public Task<List<int>> ObtenerClientesAsociadosAutomaticos(int idProductora)
            => _repo.ObtenerClientesAsociadosAutomaticos(idProductora);
    }
}