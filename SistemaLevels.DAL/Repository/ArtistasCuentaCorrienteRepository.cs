using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class ArtistasCuentaCorrienteRepository : IArtistasCuentaCorrienteRepository
    {
        private readonly SistemaLevelsContext _db;

        private const string TIPO_MOV_PAGO = "PAGO ARTISTA";
        private const string TIPO_MOV_AJUSTE = "AJUSTE ARTISTA";

        public ArtistasCuentaCorrienteRepository(SistemaLevelsContext context)
        {
            _db = context;
        }

        public async Task<List<(Artista artista, decimal saldo)>> ListarArtistas(string? buscar)
        {
            var artistasQuery = _db.Artistas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                artistasQuery = artistasQuery.Where(x =>
                    x.NombreArtistico.Contains(buscar) ||
                    x.Nombre.Contains(buscar));
            }

            var artistas = await artistasQuery
                .OrderBy(x => x.NombreArtistico)
                .ToListAsync();

            var ids = artistas.Select(x => x.Id).ToList();

            var saldos = await _db.ArtistasCuentaCorrientes
                .Where(x => ids.Contains(x.IdArtista))
                .GroupBy(x => x.IdArtista)
                .Select(g => new
                {
                    IdArtista = g.Key,
                    Saldo = g.Sum(x => x.Debe - x.Haber)
                })
                .ToListAsync();

            var saldoDict = saldos.ToDictionary(x => x.IdArtista, x => x.Saldo);

            var resultado = artistas
                .Select(a =>
                (
                    artista: a,
                    saldo: saldoDict.ContainsKey(a.Id) ? saldoDict[a.Id] : 0
                ))
                .ToList();

            return resultado;
        }

        public async Task<(ArtistasCuentaCorriente? mov, string? cuenta, decimal saldo)> ObtenerMovimiento(int id)
        {
            var mov = await _db.ArtistasCuentaCorrientes
                .Include(x => x.IdMonedaNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (mov == null)
                return (null, null, 0);

            /* =====================================
               MAPEO TIPO MOVIMIENTO
            ===================================== */

            string tipoCaja = mov.TipoMov switch
            {
                "PAGO ARTISTA" => "PAGO_ARTISTA",
                "AJUSTE ARTISTA" => "AJUSTE_ARTISTA",
                "COMISION COBRO" => "COBRO",
                _ => ""
            };

            string? cuenta = null;

            if (!string.IsNullOrEmpty(tipoCaja))
            {
                cuenta = await _db.Cajas
                    .Where(x => x.IdMov == mov.IdMov && x.TipoMov.Contains(tipoCaja))
                    .Join(
                        _db.MonedasCuentas,
                        c => c.IdCuenta,
                        cu => cu.Id,
                        (c, cu) => cu.Nombre
                    )
                    .FirstOrDefaultAsync();
            }

            /* =====================================
               CALCULAR SALDO HASTA ESTE MOVIMIENTO
            ===================================== */

            var saldo = await _db.ArtistasCuentaCorrientes
                .Where(x =>
                    x.IdArtista == mov.IdArtista &&
                    x.IdMoneda == mov.IdMoneda &&
                    (
                        x.Fecha < mov.Fecha ||
                        (x.Fecha == mov.Fecha && x.Id <= mov.Id)
                    )
                )
                .SumAsync(x => x.Debe - x.Haber);

            return (mov, cuenta, saldo);
        }
        public async Task<List<ArtistasCuentaCorriente>> Movimientos(
            int idArtista,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto)
        {
            var query = _db.ArtistasCuentaCorrientes
                .Include(x => x.IdMonedaNavigation)
                .Where(x => x.IdArtista == idArtista)
                .AsQueryable();

            if (idMoneda.HasValue)
                query = query.Where(x => x.IdMoneda == idMoneda);

            if (desde.HasValue)
                query = query.Where(x => x.Fecha >= desde);

            if (hasta.HasValue)
                query = query.Where(x => x.Fecha <= hasta);

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
            int idArtista,
            int? idMoneda,
            DateTime? desde)
        {
            if (!desde.HasValue)
                return 0;

            var query = _db.ArtistasCuentaCorrientes
                .Where(x => x.IdArtista == idArtista && x.Fecha < desde);

            if (idMoneda.HasValue)
                query = query.Where(x => x.IdMoneda == idMoneda);

            return await query.SumAsync(x => x.Debe - x.Haber);
        }

        public async Task<(decimal debe, decimal haber, int cantidad)> Resumen(
            int idArtista,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto)
        {
            var query = _db.ArtistasCuentaCorrientes
                .Where(x => x.IdArtista == idArtista);

            if (idMoneda.HasValue)
                query = query.Where(x => x.IdMoneda == idMoneda);

            if (desde.HasValue)
                query = query.Where(x => x.Fecha >= desde);

            if (hasta.HasValue)
                query = query.Where(x => x.Fecha <= hasta);

            if (!string.IsNullOrWhiteSpace(tipoMov))
                query = query.Where(x => x.TipoMov == tipoMov);

            if (!string.IsNullOrWhiteSpace(texto))
                query = query.Where(x => x.Concepto.Contains(texto));

            var debe = await query.SumAsync(x => x.Debe);
            var haber = await query.SumAsync(x => x.Haber);
            var cantidad = await query.CountAsync();

            return (debe, haber, cantidad);
        }

        public async Task<bool> RegistrarPago(
            int idArtista,
            int idMoneda,
            int idCuenta,
            DateTime fecha,
            string concepto,
            decimal importe,
            int idUsuario)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var mov = new ArtistasCuentaCorriente
                {
                    IdArtista = idArtista,
                    IdMoneda = idMoneda,
                    TipoMov = TIPO_MOV_PAGO,
                    Fecha = fecha,
                    Concepto = concepto,
                    Debe = 0,
                    Haber = importe,
                    IdUsuarioRegistra = idUsuario,
                    FechaRegistra = DateTime.Now
                };

                _db.ArtistasCuentaCorrientes.Add(mov);
                await _db.SaveChangesAsync();

                var caja = new Caja
                {
                    TipoMov = TIPO_MOV_PAGO,
                    IdMov = mov.Id,
                    Fecha = fecha,
                    Concepto = $"Pago artista {concepto}",
                    IdMoneda = idMoneda,
                    IdCuenta = idCuenta,
                    Ingreso = 0,
                    Egreso = importe,
                    Saldo = 0,
                    IdUsuarioRegistra = idUsuario,
                    FechaRegistra = DateTime.Now
                };

                _db.Cajas.Add(caja);

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

        public async Task<bool> RegistrarAjuste(
            int idArtista,
            int idMoneda,
            DateTime fecha,
            string concepto,
            decimal debe,
            decimal haber,
            int idUsuario)
        {
            var mov = new ArtistasCuentaCorriente
            {
                IdArtista = idArtista,
                IdMoneda = idMoneda,
                TipoMov = TIPO_MOV_AJUSTE,
                Fecha = fecha,
                Concepto = concepto,
                Debe = debe,
                Haber = haber,
                IdUsuarioRegistra = idUsuario,
                FechaRegistra = DateTime.Now
            };

            _db.ArtistasCuentaCorrientes.Add(mov);

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var mov = await _db.ArtistasCuentaCorrientes
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (mov == null)
                    return false;

                var caja = await _db.Cajas
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == TIPO_MOV_PAGO &&
                        x.IdMov == mov.Id);

                if (caja != null)
                    _db.Cajas.Remove(caja);

                _db.ArtistasCuentaCorrientes.Remove(mov);

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
    }
}