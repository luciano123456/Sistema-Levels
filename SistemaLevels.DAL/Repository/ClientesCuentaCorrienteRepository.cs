using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class ClientesCuentaCorrienteRepository : IClientesCuentaCorrienteRepository
    {
        private readonly SistemaLevelsContext _db;

        private const string TIPO_MOV_COBRO = "COBRO CLIENTE";
        private const string TIPO_MOV_AJUSTE = "AJUSTE CLIENTE";


        public ClientesCuentaCorrienteRepository(SistemaLevelsContext context)
        {
            _db = context;
        }

        public async Task<List<(Cliente cliente, decimal saldo)>> ListarClientes(string? buscar)
        {
            var query = _db.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
                query = query.Where(x => x.Nombre.Contains(buscar));

            var clientes = await query.OrderBy(x => x.Nombre).ToListAsync();

            var ids = clientes.Select(x => x.Id).ToList();

            var saldos = await _db.ClientesCuentaCorrientes
                .Where(x => ids.Contains(x.IdCliente))
                .GroupBy(x => x.IdCliente)
                .Select(g => new
                {
                    IdCliente = g.Key,
                    Saldo = g.Sum(x => x.Debe - x.Haber)
                })
                .ToListAsync();

            var dict = saldos.ToDictionary(x => x.IdCliente, x => x.Saldo);

            return clientes
                .Select(c => (
                    cliente: c,
                    saldo: dict.ContainsKey(c.Id) ? dict[c.Id] : 0
                ))
                .ToList();
        }

        public async Task<(ClientesCuentaCorriente? mov, string? cuenta, decimal saldo)> ObtenerMovimiento(int id)
        {
            var mov = await _db.ClientesCuentaCorrientes
                .Include(x => x.IdMonedaNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (mov == null)
                return (null, null, 0);

            /* =====================================
               MAPEO TIPOS MOVIMIENTO
            ===================================== */

            string tipoCaja = mov.TipoMov switch
            {
                "COBRO" => "COBRO",
                "VENTA" => "VENTA",
                "AJUSTE" => "AJUSTE_CLIENTE",
                _ => ""
            };

            string? cuenta = null;

            if (!string.IsNullOrEmpty(tipoCaja))
            {
                cuenta = await _db.Cajas
                    .Where(x => x.IdMov == mov.IdMov && x.TipoMov == tipoCaja)
                    .Join(
                        _db.MonedasCuentas,
                        c => c.IdCuenta,
                        cu => cu.Id,
                        (c, cu) => cu.Nombre
                    )
                    .FirstOrDefaultAsync();
            }

            var saldo = await _db.ClientesCuentaCorrientes
                .Where(x =>
                    x.IdCliente == mov.IdCliente &&
                    x.IdMoneda == mov.IdMoneda &&
                    (
                        x.Fecha < mov.Fecha ||
                        (x.Fecha == mov.Fecha && x.Id <= mov.Id)
                    )
                )
                .SumAsync(x => x.Debe - x.Haber);

            return (mov, cuenta, saldo);
        }

        public async Task<List<ClientesCuentaCorriente>> Movimientos(
            int idCliente,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto)
        {
            var query = _db.ClientesCuentaCorrientes
                .Include(x => x.IdMonedaNavigation)
                .Where(x => x.IdCliente == idCliente);

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
            int idCliente,
            int? idMoneda,
            DateTime? desde)
        {
            if (!desde.HasValue) return 0;

            var query = _db.ClientesCuentaCorrientes
                .Where(x => x.IdCliente == idCliente && x.Fecha < desde);

            if (idMoneda.HasValue)
                query = query.Where(x => x.IdMoneda == idMoneda);

            return await query.SumAsync(x => x.Debe - x.Haber);
        }

        public async Task<(decimal debe, decimal haber, int cantidad)> Resumen(
            int idCliente,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto)
        {
            var query = _db.ClientesCuentaCorrientes
                .Where(x => x.IdCliente == idCliente);

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

        public async Task<bool> RegistrarCobro(
            int idCliente,
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
                var mov = new ClientesCuentaCorriente
                {
                    IdCliente = idCliente,
                    IdMoneda = idMoneda,
                    TipoMov = TIPO_MOV_COBRO,
                    Fecha = fecha,
                    Concepto = concepto,
                    Debe = 0,
                    Haber = importe,
                    IdUsuarioRegistra = idUsuario,
                    FechaRegistra = DateTime.Now
                };

                _db.ClientesCuentaCorrientes.Add(mov);
                await _db.SaveChangesAsync();

                var caja = new Caja
                {
                    TipoMov = TIPO_MOV_COBRO,
                    IdMov = mov.Id,
                    Fecha = fecha,
                    Concepto = $"Cobro cliente {concepto}",
                    IdMoneda = idMoneda,
                    IdCuenta = idCuenta,
                    Ingreso = importe,
                    Egrreso = 0,
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
            int idCliente,
            int idMoneda,
            DateTime fecha,
            string concepto,
            decimal debe,
            decimal haber,
            int idUsuario)
        {
            var mov = new ClientesCuentaCorriente
            {
                IdCliente = idCliente,
                IdMoneda = idMoneda,
                TipoMov = TIPO_MOV_AJUSTE,
                Fecha = fecha,
                Concepto = concepto,
                Debe = debe,
                Haber = haber,
                IdUsuarioRegistra = idUsuario,
                FechaRegistra = DateTime.Now
            };

            _db.ClientesCuentaCorrientes.Add(mov);

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var mov = await _db.ClientesCuentaCorrientes
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (mov == null) return false;

                var caja = await _db.Cajas
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == TIPO_MOV_COBRO &&
                        x.IdMov == mov.Id);

                if (caja != null)
                    _db.Cajas.Remove(caja);

                _db.ClientesCuentaCorrientes.Remove(mov);

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