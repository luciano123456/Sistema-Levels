using Microsoft.EntityFrameworkCore;
using SistemaLevels.BLL.Common;
using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class ClientesService : IClientesService
    {
        private readonly IClientesRepository<Cliente> _repo;

        public ClientesService(IClientesRepository<Cliente> repo)
        {
            _repo = repo;
        }

        /* ================= INSERTAR ================= */

        public async Task<ServiceResult> Insertar(
            Cliente model,
            List<int> productorasIds)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre) ||
                string.IsNullOrWhiteSpace(model.Dni))
            {
                return ServiceResult.Error(
                    "Debe completar los campos obligatorios.",
                    "validacion");
            }

            var dup = await _repo.BuscarDuplicado(
                null,
                model.Nombre,
                model.NumeroDocumento,
                model.Dni);

            if (dup != null)
            {
                return ServiceResult.Error(
                    $"Ya existe un cliente: '{dup.Nombre}'.",
                    "duplicado",
                    dup.Id);
            }

            var ok = await _repo.Insertar(model, productorasIds);

            return ok
                ? ServiceResult.Success("Cliente registrado correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        /* ================= ACTUALIZAR ================= */

        public async Task<ServiceResult> Actualizar(
            Cliente model,
            List<int> productorasIds)
        {
            var dup = await _repo.BuscarDuplicado(
                model.Id,
                model.Nombre,
                model.NumeroDocumento,
                model.Dni);

            if (dup != null)
            {
                return ServiceResult.Error(
                    $"Ya existe un cliente: '{dup.Nombre}'.",
                    "duplicado",
                    dup.Id);
            }

            var ok = await _repo.Actualizar(model, productorasIds);

            return ok
                ? ServiceResult.Success("Cliente modificado correctamente")
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

                return ServiceResult.Success("Cliente eliminado correctamente");
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

        public Task<Cliente?> Obtener(int id)
            => _repo.Obtener(id);

        public Task<IQueryable<Cliente>> ObtenerTodos()
            => _repo.ObtenerTodos();
    }
}