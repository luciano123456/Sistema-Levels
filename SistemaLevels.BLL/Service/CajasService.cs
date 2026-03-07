using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class CajaService : ICajaService
    {
        private readonly ICajaRepository _repo;

        public CajaService(ICajaRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Caja>> Movimientos(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idMoneda,
            int? idCuenta,
            string? tipoMov,
            string? texto)
            => _repo.Movimientos(fechaDesde, fechaHasta, idMoneda, idCuenta, tipoMov, texto);

        public Task<decimal> SaldoAnterior(
            DateTime? fechaDesde,
            int? idMoneda,
            int? idCuenta)
            => _repo.SaldoAnterior(fechaDesde, idMoneda, idCuenta);

        public Task<(decimal ingresos, decimal egresos, int cantidad)> Resumen(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idMoneda,
            int? idCuenta,
            string? tipoMov,
            string? texto)
            => _repo.Resumen(fechaDesde, fechaHasta, idMoneda, idCuenta, tipoMov, texto);

        public Task<(Caja? mov, decimal saldo, string origen, bool puedeEditar, bool puedeEliminar, string? tipoTransferencia)> ObtenerMovimiento(int id)
            => _repo.ObtenerMovimiento(id);

        public Task<CajasTransferenciasCuenta?> ObtenerTransferencia(int id)
            => _repo.ObtenerTransferencia(id);

        public Task<bool> RegistrarIngresoManual(
            DateTime fecha,
            int idMoneda,
            int idCuenta,
            string concepto,
            decimal importe,
            int idUsuario)
            => _repo.RegistrarIngresoManual(fecha, idMoneda, idCuenta, concepto, importe, idUsuario);

        public Task<bool> RegistrarEgresoManual(
            DateTime fecha,
            int idMoneda,
            int idCuenta,
            string concepto,
            decimal importe,
            int idUsuario)
            => _repo.RegistrarEgresoManual(fecha, idMoneda, idCuenta, concepto, importe, idUsuario);

        public Task<bool> ActualizarMovimientoManual(
            int id,
            DateTime fecha,
            int idMoneda,
            int idCuenta,
            string concepto,
            decimal importe,
            int idUsuario)
            => _repo.ActualizarMovimientoManual(id, fecha, idMoneda, idCuenta, concepto, importe, idUsuario);

        public Task<bool> RegistrarTransferencia(
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
            => _repo.RegistrarTransferencia(
                fecha, idMonedaOrigen, idCuentaOrigen, importeOrigen,
                idMonedaDestino, idCuentaDestino, importeDestino,
                cotizacion, notaInterna, idUsuario);

        public Task<bool> ActualizarTransferencia(
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
            => _repo.ActualizarTransferencia(
                idTransferencia, fecha, idMonedaOrigen, idCuentaOrigen, importeOrigen,
                idMonedaDestino, idCuentaDestino, importeDestino, cotizacion, notaInterna, idUsuario);

        public Task<bool> Eliminar(int id, int idUsuario)
            => _repo.Eliminar(id, idUsuario);
    }
}