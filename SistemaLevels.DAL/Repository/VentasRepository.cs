using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class VentasRepository : IVentasRepository<Venta>
    {
        private readonly SistemaLevelsContext _db;

        public VentasRepository(SistemaLevelsContext context)
        {
            _db = context;
        }

        public async Task<bool> Insertar(
            Venta venta,
            List<VentasArtista> artistas,
            List<VentasPersonal> personal,
            List<VentasCobro> cobros)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                _db.Ventas.Add(venta);
                await _db.SaveChangesAsync();

                await SincronizarArtistas(venta.Id, artistas);
                await SincronizarPersonal(venta.Id, personal);
                await SincronizarCobros(venta.Id, cobros);

                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> Actualizar(
            Venta venta,
            List<VentasArtista> artistas,
            List<VentasPersonal> personal,
            List<VentasCobro> cobros)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var entity = await _db.Ventas.FirstOrDefaultAsync(x => x.Id == venta.Id);
                if (entity == null) return false;

                // Campos
                entity.IdPresupuesto = null;
                entity.IdClienteCc = null;

                entity.Fecha = venta.Fecha;
                entity.IdUbicacion = venta.IdUbicacion;
                entity.NombreEvento = venta.NombreEvento;
                entity.Duracion = venta.Duracion;

                entity.IdCliente = venta.IdCliente;
                entity.IdProductora = venta.IdProductora;
                entity.IdMoneda = venta.IdMoneda;
                entity.IdEstado = venta.IdEstado;

                entity.ImporteTotal = venta.ImporteTotal;
                entity.ImporteAbonado = venta.ImporteAbonado;
                entity.Saldo = venta.Saldo;

                entity.NotaInterna = venta.NotaInterna;
                entity.NotaCliente = venta.NotaCliente;

                entity.IdTipoContrato = venta.IdTipoContrato;
                entity.IdOpExclusividad = venta.IdOpExclusividad;
                entity.DiasPrevios = venta.DiasPrevios;
                entity.FechaHasta = venta.FechaHasta;

                entity.IdUsuarioModifica = venta.IdUsuarioModifica;
                entity.FechaModifica = DateTime.Now;

                await _db.SaveChangesAsync();

                await SincronizarArtistas(entity.Id, artistas);
                await SincronizarPersonal(entity.Id, personal);
                await SincronizarCobros(entity.Id, cobros);

                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var venta = await _db.Ventas.FirstOrDefaultAsync(x => x.Id == id);
                if (venta == null) return false;

                var arts = await _db.VentasArtistas.Where(x => x.IdVenta == id).ToListAsync();
                var pers = await _db.VentasPersonals.Where(x => x.IdVenta == id).ToListAsync();

                var cobros = await _db.VentasCobros.Where(x => x.IdVenta == id).ToListAsync();
                var cobrosIds = cobros.Select(x => x.Id).ToList();

                // Comisiones por cobro (si existen)
                var comis = await _db.VentasCobrosComisiones
                    .Where(x => cobrosIds.Contains(x.IdVentaCobro))
                    .ToListAsync();

                _db.VentasCobrosComisiones.RemoveRange(comis);
                _db.VentasCobros.RemoveRange(cobros);
                _db.VentasArtistas.RemoveRange(arts);
                _db.VentasPersonals.RemoveRange(pers);

                _db.Ventas.Remove(venta);

                await _db.SaveChangesAsync();
                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        public async Task<Venta?> Obtener(int id)
        {
            return await _db.Ventas
                .AsNoTracking()

                .Include(x => x.IdClienteNavigation)
                .Include(x => x.IdProductoraNavigation)
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdEstadoNavigation)
                .Include(x => x.IdUbicacionNavigation)
                .Include(x => x.IdTipoContratoNavigation)
                .Include(x => x.IdOpExclusividadNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)

                .Include(x => x.VentasArtista)
                    .ThenInclude(va => va.IdArtistaNavigation)
                .Include(x => x.VentasArtista)
                    .ThenInclude(va => va.IdRepresentanteNavigation)

                .Include(x => x.VentasPersonals)
                    .ThenInclude(vp => vp.IdPersonalNavigation)
                .Include(x => x.VentasPersonals)
                    .ThenInclude(vp => vp.IdCargoNavigation)
                .Include(x => x.VentasPersonals)
                    .ThenInclude(vp => vp.IdTipoComisionNavigation)

                .Include(x => x.VentasCobros)
                    .ThenInclude(c => c.IdMonedaNavigation)
                .Include(x => x.VentasCobros)
                    .ThenInclude(c => c.IdCuentaNavigation)

                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IQueryable<Venta>> ObtenerTodos()
        {
            var q = _db.Ventas
                .AsNoTracking()
                .Include(x => x.IdClienteNavigation)
                .Include(x => x.IdProductoraNavigation)
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdEstadoNavigation)
                .Include(x => x.IdUbicacionNavigation)
                .Include(x => x.IdTipoContratoNavigation)
                .Include(x => x.IdOpExclusividadNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation);

            return await Task.FromResult(q);
        }

        public async Task<IQueryable<Venta>> ObtenerPorCliente(int idCliente)
        {
            var q = _db.Ventas
                .AsNoTracking()
                .Where(x => x.IdCliente == idCliente)
                .Include(x => x.IdClienteNavigation)
                .Include(x => x.IdProductoraNavigation)
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdEstadoNavigation)
                .Include(x => x.IdUbicacionNavigation)
                .Include(x => x.IdTipoContratoNavigation)
                .Include(x => x.IdOpExclusividadNavigation);

            return await Task.FromResult(q);
        }

        private async Task SincronizarArtistas(int idVenta, List<VentasArtista> artistas)
        {
            artistas ??= new();

            artistas = artistas
                .Where(x => x.IdArtista > 0 && x.IdRepresentante > 0)
                .ToList();

            var actuales = await _db.VentasArtistas
                .Where(x => x.IdVenta == idVenta)
                .ToListAsync();

            _db.VentasArtistas.RemoveRange(actuales);

            foreach (var it in artistas)
            {
                _db.VentasArtistas.Add(new VentasArtista
                {
                    IdVenta = idVenta,
                    IdArtista = it.IdArtista,
                    IdRepresentante = it.IdRepresentante,
                    PorcComision = it.PorcComision,
                    TotalComision = it.TotalComision,
                    IdArtistaCc = 0
                });
            }

            await _db.SaveChangesAsync();
        }

        private async Task SincronizarPersonal(int idVenta, List<VentasPersonal> personal)
        {
            personal ??= new();

            personal = personal
                .Where(x => x.IdPersonal > 0 && x.IdCargo > 0 && x.IdTipoComision > 0)
                .ToList();

            var actuales = await _db.VentasPersonals
                .Where(x => x.IdVenta == idVenta)
                .ToListAsync();

            _db.VentasPersonals.RemoveRange(actuales);

            foreach (var it in personal)
            {
                _db.VentasPersonals.Add(new VentasPersonal
                {
                    IdVenta = idVenta,

                    IdPersonal = it.IdPersonal,
                    IdCargo = it.IdCargo,
                    IdTipoComision = it.IdTipoComision,

                    PorcComision = it.PorcComision,
                    TotalComision = it.TotalComision,

                    IdUsuarioRegistra = it.IdUsuarioRegistra,
                    FechaRegistra = it.FechaRegistra == default ? DateTime.Now : it.FechaRegistra,
                    IdUsuarioModifica = it.IdUsuarioModifica,
                    FechaModifica = it.FechaModifica
                });
            }

            await _db.SaveChangesAsync();
        }

        private async Task SincronizarCobros(int idVenta, List<VentasCobro> cobros)
        {
            cobros ??= new();

            // cobros opcionales: si vienen 0, se borran los actuales (es el estado real)
            var actuales = await _db.VentasCobros
                .Where(x => x.IdVenta == idVenta)
                .ToListAsync();

            // borrar comisiones asociadas a cobros actuales
            var actualesIds = actuales.Select(x => x.Id).ToList();
            if (actualesIds.Count > 0)
            {
                var comis = await _db.VentasCobrosComisiones
                    .Where(x => actualesIds.Contains(x.IdVentaCobro))
                    .ToListAsync();

                _db.VentasCobrosComisiones.RemoveRange(comis);
            }

            _db.VentasCobros.RemoveRange(actuales);

            foreach (var c in cobros)
            {
                _db.VentasCobros.Add(new VentasCobro
                {
                    IdVenta = idVenta,

                    IdClienteCc = null,
                    IdArtistaCc = null,
                    IdCaja = null,

                    Fecha = c.Fecha == default ? DateTime.Now : c.Fecha,
                    IdMoneda = c.IdMoneda,
                    IdCuenta = c.IdCuenta,
                    Importe = c.Importe,

                    Cotizacion = c.Cotizacion <= 0 ? 1 : c.Cotizacion,
                    Conversion = c.Conversion <= 0 ? c.Importe : c.Conversion,

                    NotaCliente = c.NotaCliente,
                    NotaInterna = c.NotaInterna,

                    IdUsuarioRegistra = c.IdUsuarioRegistra,
                    FechaRegistra = c.FechaRegistra == default ? DateTime.Now : c.FechaRegistra,
                    IdUsuarioModifica = c.IdUsuarioModifica,
                    FechaModifica = c.FechaModifica
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}