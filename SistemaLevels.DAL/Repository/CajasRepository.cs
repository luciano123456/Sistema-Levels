using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class CajaRepository : ICajaRepository
    {
        private readonly SistemaLevelsContext _db;

        private const string TIPO_INGRESO_MANUAL = "INGRESO MANUAL";
        private const string TIPO_EGRESO_MANUAL = "EGRESO MANUAL";
        private const string TIPO_TRANSFERENCIA = "TRANSFERENCIA";

        public CajaRepository(SistemaLevelsContext context)
        {
            _db = context;
        }

        public async Task<List<Caja>> Movimientos(
     DateTime? fechaDesde,
     DateTime? fechaHasta,
     int? idMoneda,
     int? idCuenta,
     string? tipoMov,
     string? texto)
        {
            var query = _db.Cajas
                .AsNoTracking()
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdCuentaNavigation)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(x => x.Fecha >= fechaDesde.Value.Date);

            if (fechaHasta.HasValue)
                query = query.Where(x => x.Fecha <= fechaHasta.Value.Date.AddDays(1).AddTicks(-1));

            if (idMoneda.HasValue)
                query = query.Where(x => x.IdMoneda == idMoneda.Value);

            if (idCuenta.HasValue)
                query = query.Where(x => x.IdCuenta == idCuenta.Value);

            if (!string.IsNullOrWhiteSpace(tipoMov))
                query = query.Where(x => x.TipoMov == tipoMov);

            if (!string.IsNullOrWhiteSpace(texto))
                query = query.Where(x => x.Concepto.Contains(texto));

            return await query
                .OrderBy(x => x.Fecha)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<decimal> SaldoAnterior(
            DateTime? fechaDesde,
            int? idMoneda,
            int? idCuenta)
        {
            if (!fechaDesde.HasValue)
                return 0;

            var query = _db.Cajas
                .Where(x => x.Fecha < fechaDesde.Value);

            if (idMoneda.HasValue)
                query = query.Where(x => x.IdMoneda == idMoneda.Value);

            if (idCuenta.HasValue)
                query = query.Where(x => x.IdCuenta == idCuenta.Value);

            return await query.SumAsync(x => x.Ingreso - x.Egreso);
        }

        public async Task<(decimal ingresos, decimal egresos, int cantidad)> Resumen(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idMoneda,
            int? idCuenta,
            string? tipoMov,
            string? texto)
        {
            var query = _db.Cajas.AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(x => x.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(x => x.Fecha <= fechaHasta.Value);

            if (idMoneda.HasValue)
                query = query.Where(x => x.IdMoneda == idMoneda.Value);

            if (idCuenta.HasValue)
                query = query.Where(x => x.IdCuenta == idCuenta.Value);

            if (!string.IsNullOrWhiteSpace(tipoMov))
                query = query.Where(x => x.TipoMov == tipoMov);

            if (!string.IsNullOrWhiteSpace(texto))
                query = query.Where(x => x.Concepto.Contains(texto));

            var ingresos = await query.SumAsync(x => x.Ingreso);
            var egresos = await query.SumAsync(x => x.Egreso);
            var cantidad = await query.CountAsync();

            return (ingresos, egresos, cantidad);
        }

        public async Task<(Caja? mov, decimal saldo, string origen, bool puedeEditar, bool puedeEliminar, string? tipoTransferencia)> ObtenerMovimiento(int id)
        {
            var mov = await _db.Cajas
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdCuentaNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (mov == null)
                return (null, 0, "", false, false, null);

            var saldo = await _db.Cajas
                .Where(x =>
                    (x.Fecha < mov.Fecha) ||
                    (x.Fecha == mov.Fecha && x.Id <= mov.Id))
                .Where(x => x.IdMoneda == mov.IdMoneda && x.IdCuenta == mov.IdCuenta)
                .SumAsync(x => x.Ingreso - x.Egreso);

            var origen = ObtenerOrigen(mov.TipoMov);
            var puedeEditar = EsEditable(mov.TipoMov);
            var puedeEliminar = EsEditable(mov.TipoMov);

            string? tipoTransferencia = null;

            if (mov.TipoMov == TIPO_TRANSFERENCIA && mov.IdMov.HasValue)
            {
                tipoTransferencia = mov.Ingreso > 0 ? "ENTRADA" : "SALIDA";
            }

            return (mov, saldo, origen, puedeEditar, puedeEliminar, tipoTransferencia);
        }

        public async Task<CajasTransferenciasCuenta?> ObtenerTransferencia(int id)
        {
            return await _db.CajasTransferenciasCuentas
                .Include(x => x.IdMonedaOrigenNavigation)
                .Include(x => x.IdCuentaOrigenNavigation)
                .Include(x => x.IdMonedaDestinoNavigation)
                .Include(x => x.IdCuentaDestinoNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> RegistrarIngresoManual(
            DateTime fecha,
            int idMoneda,
            int idCuenta,
            string concepto,
            decimal importe,
            int idUsuario)
        {
            if (importe <= 0) return false;

            var mov = new Caja
            {
                TipoMov = TIPO_INGRESO_MANUAL,
                IdMov = null,
                Fecha = fecha,
                Concepto = concepto,
                IdMoneda = idMoneda,
                IdCuenta = idCuenta,
                Ingreso = importe,
                Egreso = 0,
                Saldo = 0,
                IdUsuarioRegistra = idUsuario,
                FechaRegistra = DateTime.Now
            };

            _db.Cajas.Add(mov);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegistrarEgresoManual(
            DateTime fecha,
            int idMoneda,
            int idCuenta,
            string concepto,
            decimal importe,
            int idUsuario)
        {
            if (importe <= 0) return false;

            var mov = new Caja
            {
                TipoMov = TIPO_EGRESO_MANUAL,
                IdMov = null,
                Fecha = fecha,
                Concepto = concepto,
                IdMoneda = idMoneda,
                IdCuenta = idCuenta,
                Ingreso = 0,
                Egreso = importe,
                Saldo = 0,
                IdUsuarioRegistra = idUsuario,
                FechaRegistra = DateTime.Now
            };

            _db.Cajas.Add(mov);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarMovimientoManual(
            int id,
            DateTime fecha,
            int idMoneda,
            int idCuenta,
            string concepto,
            decimal importe,
            int idUsuario)
        {
            if (importe <= 0) return false;

            var mov = await _db.Cajas.FirstOrDefaultAsync(x => x.Id == id);
            if (mov == null) return false;
            if (!EsEditable(mov.TipoMov)) return false;

            mov.Fecha = fecha;
            mov.Concepto = concepto;
            mov.IdMoneda = idMoneda;
            mov.IdCuenta = idCuenta;
            mov.IdUsuarioModifica = idUsuario;
            mov.FechaModifica = DateTime.Now;

            if (mov.TipoMov == TIPO_INGRESO_MANUAL)
            {
                mov.Ingreso = importe;
                mov.Egreso = 0;
            }
            else if (mov.TipoMov == TIPO_EGRESO_MANUAL)
            {
                mov.Ingreso = 0;
                mov.Egreso = importe;
            }
            else
            {
                return false;
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegistrarTransferencia(
            DateTime fecha,
            int idMonedaOrigen,
            int idCuentaOrigen,
            decimal importeOrigen,
            int idMonedaDestino,
            int idCuentaDestino,
            decimal importeDestino,
            decimal cotizacion,
            string notaInterna,
            int idUsuario)
        {
            if (importeOrigen <= 0 || importeDestino <= 0)
                return false;

            await using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var transferencia = new CajasTransferenciasCuenta
                {
                    Fecha = fecha,
                    IdMonedaOrigen = idMonedaOrigen,
                    IdCuentaOrigen = idCuentaOrigen,
                    ImporteOrigen = importeOrigen,
                    IdMonedaDestino = idMonedaDestino,
                    IdCuentaDestino = idCuentaDestino,
                    ImporteDestino = importeDestino,
                    Cotizacion = cotizacion <= 0 ? 1 : cotizacion,
                    NotaInterna = notaInterna ?? "",
                    IdUsuarioRegistra = idUsuario,
                    FechaRegistra = DateTime.Now
                };

                _db.CajasTransferenciasCuentas.Add(transferencia);
                await _db.SaveChangesAsync();

                var salida = new Caja
                {
                    TipoMov = TIPO_TRANSFERENCIA,
                    IdMov = transferencia.Id,
                    Fecha = fecha,
                    Concepto = $"Transferencia salida - {notaInterna}",
                    IdMoneda = idMonedaOrigen,
                    IdCuenta = idCuentaOrigen,
                    Ingreso = 0,
                    Egreso = importeOrigen,
                    Saldo = 0,
                    IdUsuarioRegistra = idUsuario,
                    FechaRegistra = DateTime.Now
                };

                var entrada = new Caja
                {
                    TipoMov = TIPO_TRANSFERENCIA,
                    IdMov = transferencia.Id,
                    Fecha = fecha,
                    Concepto = $"Transferencia entrada - {notaInterna}",
                    IdMoneda = idMonedaDestino,
                    IdCuenta = idCuentaDestino,
                    Ingreso = importeDestino,
                    Egreso = 0,
                    Saldo = 0,
                    IdUsuarioRegistra = idUsuario,
                    FechaRegistra = DateTime.Now
                };

                _db.Cajas.Add(salida);
                _db.Cajas.Add(entrada);

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

        public async Task<bool> ActualizarTransferencia(
            int idTransferencia,
            DateTime fecha,
            int idMonedaOrigen,
            int idCuentaOrigen,
            decimal importeOrigen,
            int idMonedaDestino,
            int idCuentaDestino,
            decimal importeDestino,
            decimal cotizacion,
            string notaInterna,
            int idUsuario)
        {
            if (importeOrigen <= 0 || importeDestino <= 0)
                return false;

            await using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var transferencia = await _db.CajasTransferenciasCuentas
                    .FirstOrDefaultAsync(x => x.Id == idTransferencia);

                if (transferencia == null)
                    return false;

                var movimientos = await _db.Cajas
                    .Where(x => x.TipoMov == TIPO_TRANSFERENCIA && x.IdMov == idTransferencia)
                    .OrderBy(x => x.Id)
                    .ToListAsync();

                if (movimientos.Count != 2)
                    return false;

                transferencia.Fecha = fecha;
                transferencia.IdMonedaOrigen = idMonedaOrigen;
                transferencia.IdCuentaOrigen = idCuentaOrigen;
                transferencia.ImporteOrigen = importeOrigen;
                transferencia.IdMonedaDestino = idMonedaDestino;
                transferencia.IdCuentaDestino = idCuentaDestino;
                transferencia.ImporteDestino = importeDestino;
                transferencia.Cotizacion = cotizacion <= 0 ? 1 : cotizacion;
                transferencia.NotaInterna = notaInterna ?? "";
                transferencia.IdUsuarioModifica = idUsuario;
                transferencia.FechaModifica = DateTime.Now;

                var movSalida = movimientos.FirstOrDefault(x => x.Egreso > 0);
                var movEntrada = movimientos.FirstOrDefault(x => x.Ingreso > 0);

                if (movSalida == null || movEntrada == null)
                    return false;

                movSalida.Fecha = fecha;
                movSalida.IdMoneda = idMonedaOrigen;
                movSalida.IdCuenta = idCuentaOrigen;
                movSalida.Concepto = $"Transferencia salida - {notaInterna}";
                movSalida.Ingreso = 0;
                movSalida.Egreso = importeOrigen;
                movSalida.IdUsuarioModifica = idUsuario;
                movSalida.FechaModifica = DateTime.Now;

                movEntrada.Fecha = fecha;
                movEntrada.IdMoneda = idMonedaDestino;
                movEntrada.IdCuenta = idCuentaDestino;
                movEntrada.Concepto = $"Transferencia entrada - {notaInterna}";
                movEntrada.Ingreso = importeDestino;
                movEntrada.Egreso = 0;
                movEntrada.IdUsuarioModifica = idUsuario;
                movEntrada.FechaModifica = DateTime.Now;

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

        public async Task<bool> Eliminar(int id, int idUsuario)
        {
            await using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var mov = await _db.Cajas.FirstOrDefaultAsync(x => x.Id == id);
                if (mov == null)
                    return false;

                if (!EsEditable(mov.TipoMov))
                    return false;

                if (mov.TipoMov == TIPO_TRANSFERENCIA)
                {
                    if (!mov.IdMov.HasValue)
                        return false;

                    var transferencia = await _db.CajasTransferenciasCuentas
                        .FirstOrDefaultAsync(x => x.Id == mov.IdMov.Value);

                    var movimientos = await _db.Cajas
                        .Where(x => x.TipoMov == TIPO_TRANSFERENCIA && x.IdMov == mov.IdMov)
                        .ToListAsync();

                    if (movimientos.Count > 0)
                        _db.Cajas.RemoveRange(movimientos);

                    if (transferencia != null)
                        _db.CajasTransferenciasCuentas.Remove(transferencia);

                    await _db.SaveChangesAsync();
                    await trx.CommitAsync();
                    return true;
                }

                _db.Cajas.Remove(mov);
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

        private static bool EsEditable(string tipoMov)
        {
            return tipoMov == TIPO_INGRESO_MANUAL
                || tipoMov == TIPO_EGRESO_MANUAL
                || tipoMov == TIPO_TRANSFERENCIA;
        }

        private static string ObtenerOrigen(string tipoMov)
        {
            return tipoMov switch
            {
                TIPO_INGRESO_MANUAL => "MANUAL",
                TIPO_EGRESO_MANUAL => "MANUAL",
                TIPO_TRANSFERENCIA => "TRANSFERENCIA",
                "COBRO" => "VENTAS",
                "PAGO ARTISTA" => "CUENTA CORRIENTE ARTISTAS",
                "PAGO PERSONAL" => "CUENTA CORRIENTE PERSONAL",
                "PAGO CLIENTE" => "CUENTA CORRIENTE CLIENTES",
                _ => "SISTEMA"
            };
        }
    }
}