using Microsoft.EntityFrameworkCore;
using SistemaLevels.BLL.Common;
using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class PersonalSueldosService : IPersonalSueldosService
    {
        private readonly IPersonalSueldosRepository<PersonalSueldo> _repo;

        public PersonalSueldosService(IPersonalSueldosRepository<PersonalSueldo> repo)
        {
            _repo = repo;
        }

        public async Task<IQueryable<PersonalSueldo>> ListarFiltrado(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idPersonal,
            int? idMoneda,
            string? estado)
        {
            return await _repo.ListarFiltrado(
                fechaDesde,
                fechaHasta,
                idPersonal,
                idMoneda,
                estado
            );
        }

        public async Task<ServiceResult> Insertar(
            PersonalSueldo sueldo,
            List<PersonalSueldosPago> pagos)
        {
            if (sueldo.Fecha == default ||
                sueldo.IdPersonal <= 0 ||
                sueldo.IdMoneda <= 0 ||
                string.IsNullOrWhiteSpace(sueldo.Concepto) ||
                sueldo.ImporteTotal <= 0)
            {
                return ServiceResult.Error("Debe completar los campos obligatorios.", "validacion");
            }

            pagos ??= new();

            foreach (var p in pagos)
            {
                if (p.Importe <= 0)
                    return ServiceResult.Error("Hay pagos con importe inválido.", "validacion");

                if (p.IdCuenta <= 0)
                    return ServiceResult.Error("Hay pagos sin cuenta.", "validacion");

                if (p.IdMoneda <= 0)
                    return ServiceResult.Error("Hay pagos sin moneda.", "validacion");

                if (p.Fecha == default)
                    p.Fecha = DateTime.Now;

                if (p.Cotizacion <= 0)
                    p.Cotizacion = 1;

                if (p.Conversion <= 0)
                    p.Conversion = p.Importe * p.Cotizacion;
            }

            var ok = await _repo.Insertar(sueldo, pagos);

            return ok
                ? ServiceResult.Success("Sueldo registrado correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        public async Task<ServiceResult> Actualizar(
            PersonalSueldo sueldo,
            List<PersonalSueldosPago> pagos)
        {
            if (sueldo.Id <= 0)
                return ServiceResult.Error("Registro inválido.", "validacion");

            if (sueldo.Fecha == default ||
                sueldo.IdPersonal <= 0 ||
                sueldo.IdMoneda <= 0 ||
                string.IsNullOrWhiteSpace(sueldo.Concepto) ||
                sueldo.ImporteTotal <= 0)
            {
                return ServiceResult.Error("Debe completar los campos obligatorios.", "validacion");
            }

            pagos ??= new();

            foreach (var p in pagos)
            {
                if (p.Importe <= 0)
                    return ServiceResult.Error("Hay pagos con importe inválido.", "validacion");

                if (p.IdCuenta <= 0)
                    return ServiceResult.Error("Hay pagos sin cuenta.", "validacion");

                if (p.IdMoneda <= 0)
                    return ServiceResult.Error("Hay pagos sin moneda.", "validacion");

                if (p.Fecha == default)
                    p.Fecha = DateTime.Now;

                if (p.Cotizacion <= 0)
                    p.Cotizacion = 1;

                if (p.Conversion <= 0)
                    p.Conversion = p.Importe * p.Cotizacion;
            }

            var ok = await _repo.Actualizar(sueldo, pagos);

            return ok
                ? ServiceResult.Success("Sueldo modificado correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        public async Task<ServiceResult> Eliminar(int id)
        {
            try
            {
                var ok = await _repo.Eliminar(id);

                if (!ok)
                    return ServiceResult.Error("No se encontró el registro.");

                return ServiceResult.Success("Sueldo eliminado correctamente");
            }
            catch (DbUpdateException)
            {
                return ServiceResult.Error(
                    "No se puede eliminar porque posee registros relacionados.",
                    "relacion",
                    id);
            }
            catch (Exception ex)
            {
                return ServiceResult.Error($"Error inesperado. {ex.Message}");
            }
        }

        public Task<PersonalSueldo?> Obtener(int id) => _repo.Obtener(id);
        public Task<IQueryable<PersonalSueldo>> ObtenerTodos() => _repo.ObtenerTodos();
        public Task<IQueryable<PersonalSueldo>> ObtenerPorPersonal(int idPersonal) => _repo.ObtenerPorPersonal(idPersonal);
    }
}