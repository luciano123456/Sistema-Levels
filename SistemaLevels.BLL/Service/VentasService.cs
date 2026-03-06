using Microsoft.EntityFrameworkCore;
using SistemaLevels.BLL.Common;
using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class VentasService : IVentasService
    {
        private readonly IVentasRepository<Venta> _repo;

        public VentasService(IVentasRepository<Venta> repo)
        {
            _repo = repo;
        }

        public async Task<IQueryable<Venta>> ListarFiltrado(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idEstado,
            int? idArtista,
            int? idCliente)
        {
            return await _repo.ListarFiltrado(
                fechaDesde,
                fechaHasta,
                idEstado,
                idArtista,
                idCliente
            );
        }

        public async Task<ServiceResult> Insertar(
            Venta venta,
            List<VentasArtista> artistas,
            List<VentasPersonal> personal,
            List<VentasCobro> cobros)
        {
            if (venta.Fecha == default ||
                string.IsNullOrWhiteSpace(venta.NombreEvento) ||
                venta.IdCliente <= 0 ||
                venta.IdProductora <= 0 ||
                venta.IdUbicacion <= 0 ||
                venta.IdMoneda <= 0 ||
                venta.IdEstado <= 0 ||
                venta.IdTipoContrato <= 0)
            {
                return ServiceResult.Error("Debe completar los campos obligatorios.", "validacion");
            }

            venta.IdClienteCc = null;
            venta.IdPresupuesto = null;

            artistas ??= new();
            personal ??= new();
            cobros ??= new();

            foreach (var c in cobros)
            {
                if (c.Importe <= 0) return ServiceResult.Error("Hay cobros con importe inválido.", "validacion");
                if (c.IdCuenta <= 0) return ServiceResult.Error("Hay cobros sin cuenta.", "validacion");
                if (c.IdMoneda <= 0) return ServiceResult.Error("Hay cobros sin moneda.", "validacion");
                if (c.Fecha == default) c.Fecha = DateTime.Now;
                if (c.Cotizacion <= 0) c.Cotizacion = 1;
                if (c.Conversion <= 0) c.Conversion = c.Importe;
            }

            venta.ImporteAbonado = cobros.Sum(x => x.Conversion);
            venta.Saldo = venta.ImporteTotal - venta.ImporteAbonado;

            var ok = await _repo.Insertar(venta, artistas, personal, cobros);

            return ok
                ? ServiceResult.Success("Venta registrada correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        public async Task<ServiceResult> Actualizar(
            Venta venta,
            List<VentasArtista> artistas,
            List<VentasPersonal> personal,
            List<VentasCobro> cobros)
        {
            if (venta.Id <= 0)
                return ServiceResult.Error("Registro inválido.", "validacion");

            if (venta.Fecha == default ||
                string.IsNullOrWhiteSpace(venta.NombreEvento) ||
                venta.IdCliente <= 0 ||
                venta.IdProductora <= 0 ||
                venta.IdUbicacion <= 0 ||
                venta.IdMoneda <= 0 ||
                venta.IdEstado <= 0 ||
                venta.IdTipoContrato <= 0)
            {
                return ServiceResult.Error("Debe completar los campos obligatorios.", "validacion");
            }

            venta.IdClienteCc = venta.IdClienteCc;
            venta.IdPresupuesto = null;

            artistas ??= new();
            personal ??= new();
            cobros ??= new();

            foreach (var c in cobros)
            {
                if (c.Importe <= 0) return ServiceResult.Error("Hay cobros con importe inválido.", "validacion");
                if (c.IdCuenta <= 0) return ServiceResult.Error("Hay cobros sin cuenta.", "validacion");
                if (c.IdMoneda <= 0) return ServiceResult.Error("Hay cobros sin moneda.", "validacion");
                if (c.Fecha == default) c.Fecha = DateTime.Now;
                if (c.Cotizacion <= 0) c.Cotizacion = 1;
                if (c.Conversion <= 0) c.Conversion = c.Importe;
            }

            venta.ImporteAbonado = cobros.Sum(x => x.Conversion);
            venta.Saldo = venta.ImporteTotal - venta.ImporteAbonado;

            var ok = await _repo.Actualizar(venta, artistas, personal, cobros);

            return ok
                ? ServiceResult.Success("Venta modificada correctamente")
                : ServiceResult.Error("No se pudo guardar");
        }

        public async Task<ServiceResult> Eliminar(int id)
        {
            try
            {
                var ok = await _repo.Eliminar(id);
                if (!ok) return ServiceResult.Error("No se encontró el registro.");
                return ServiceResult.Success("Venta eliminada correctamente");
            }
            catch (DbUpdateException)
            {
                return ServiceResult.Error("No se puede eliminar porque posee registros relacionados.", "relacion", id);
            }
            catch
            {
                return ServiceResult.Error("Error inesperado.");
            }
        }

        public Task<Venta?> Obtener(int id) => _repo.Obtener(id);
        public Task<IQueryable<Venta>> ObtenerTodos() => _repo.ObtenerTodos();
        public Task<IQueryable<Venta>> ObtenerPorCliente(int idCliente) => _repo.ObtenerPorCliente(idCliente);
    }
}