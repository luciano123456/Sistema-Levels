using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class PersonalCuentaCorrienteRepository : IPersonalCuentaCorrienteRepository
    {
        private readonly SistemaLevelsContext _db;

        private const string TIPO_MOV_PAGO = "PAGO PERSONAL";
        private const string TIPO_MOV_AJUSTE = "AJUSTE PERSONAL";

        public PersonalCuentaCorrienteRepository(SistemaLevelsContext context)
        {
            _db = context;
        }

        public async Task<List<(Personal personal, decimal saldo)>> ListarPersonal(string? buscar)
        {
            var personalQuery = _db.Personals.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                personalQuery = personalQuery.Where(x =>
                    x.Nombre.Contains(buscar));
            }

            var personal = await personalQuery
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            var ids = personal.Select(x => x.Id).ToList();

            var saldos = await _db.PersonalCuentaCorrientes
                .Where(x => ids.Contains(x.IdPersonal))
                .GroupBy(x => x.IdPersonal)
                .Select(g => new
                {
                    IdPersonal = g.Key,
                    Saldo = g.Sum(x => x.Debe - x.Haber)
                })
                .ToListAsync();

            var saldoDict = saldos.ToDictionary(x => x.IdPersonal, x => x.Saldo);

            var resultado = personal
                .Select(p =>
                (
                    personal: p,
                    saldo: saldoDict.ContainsKey(p.Id) ? saldoDict[p.Id] : 0
                ))
                .ToList();

            return resultado;
        }

        public async Task<(PersonalCuentaCorriente? mov, string? cuenta, decimal saldo)> ObtenerMovimiento(int id)
        {
            var mov = await _db.PersonalCuentaCorrientes
                .Include(x => x.IdMonedaNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (mov == null)
                return (null, null, 0);

            string tipoCaja = mov.TipoMov switch
            {
                "PAGO PERSONAL" => "PAGO_PERSONAL",
                "AJUSTE PERSONAL" => "AJUSTE_PERSONAL",
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

            var saldo = await _db.PersonalCuentaCorrientes
                .Where(x =>
                    x.IdPersonal == mov.IdPersonal &&
                    x.IdMoneda == mov.IdMoneda &&
                    (
                        x.Fecha < mov.Fecha ||
                        (x.Fecha == mov.Fecha && x.Id <= mov.Id)
                    )
                )
                .SumAsync(x => x.Debe - x.Haber);

            return (mov, cuenta, saldo);
        }

        public async Task<List<PersonalCuentaCorriente>> Movimientos(
            int idPersonal,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto)
        {
            var query = _db.PersonalCuentaCorrientes
                .Include(x => x.IdMonedaNavigation)
                .Where(x => x.IdPersonal == idPersonal)
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
            int idPersonal,
            int? idMoneda,
            DateTime? desde)
        {
            if (!desde.HasValue)
                return 0;

            var query = _db.PersonalCuentaCorrientes
                .Where(x => x.IdPersonal == idPersonal && x.Fecha < desde);

            if (idMoneda.HasValue)
                query = query.Where(x => x.IdMoneda == idMoneda);

            return await query.SumAsync(x => x.Debe - x.Haber);
        }

        public async Task<(decimal debe, decimal haber, int cantidad)> Resumen(
            int idPersonal,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto)
        {
            var query = _db.PersonalCuentaCorrientes
                .Where(x => x.IdPersonal == idPersonal);

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
            int idPersonal,
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
                var mov = new PersonalCuentaCorriente
                {
                    IdPersonal = idPersonal,
                    IdMoneda = idMoneda,
                    TipoMov = TIPO_MOV_PAGO,
                    Fecha = fecha,
                    Concepto = concepto,
                    Debe = 0,
                    Haber = importe,
                    IdUsuarioRegistra = idUsuario,
                    FechaRegistra = DateTime.Now
                };

                _db.PersonalCuentaCorrientes.Add(mov);

                await _db.SaveChangesAsync();

                var caja = new Caja
                {
                    TipoMov = TIPO_MOV_PAGO,
                    IdMov = mov.Id,
                    Fecha = fecha,
                    Concepto = $"Pago personal {concepto}",
                    IdMoneda = idMoneda,
                    IdCuenta = idCuenta,
                    Ingreso = 0,
                    Egrreso = importe,
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
            int idPersonal,
            int idMoneda,
            DateTime fecha,
            string concepto,
            decimal debe,
            decimal haber,
            int idUsuario)
        {
            var mov = new PersonalCuentaCorriente
            {
                IdPersonal = idPersonal,
                IdMoneda = idMoneda,
                TipoMov = TIPO_MOV_AJUSTE,
                Fecha = fecha,
                Concepto = concepto,
                Debe = debe,
                Haber = haber,
                IdUsuarioRegistra = idUsuario,
                FechaRegistra = DateTime.Now
            };

            _db.PersonalCuentaCorrientes.Add(mov);

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var mov = await _db.PersonalCuentaCorrientes
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (mov == null)
                    return false;

                var caja = await _db.Cajas
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == TIPO_MOV_PAGO &&
                        x.IdMov == mov.Id);

                if (caja != null)
                    _db.Cajas.Remove(caja);

                _db.PersonalCuentaCorrientes.Remove(mov);

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