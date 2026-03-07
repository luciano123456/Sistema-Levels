using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface ICajaService
    {
        Task<List<Caja>> Movimientos(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idMoneda,
            int? idCuenta,
            string? tipoMov,
            string? texto);

        Task<decimal> SaldoAnterior(
            DateTime? fechaDesde,
            int? idMoneda,
            int? idCuenta);

        Task<(decimal ingresos, decimal egresos, int cantidad)> Resumen(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idMoneda,
            int? idCuenta,
            string? tipoMov,
            string? texto);

        Task<(Caja? mov, decimal saldo, string origen, bool puedeEditar, bool puedeEliminar, string? tipoTransferencia)> ObtenerMovimiento(int id);

        Task<CajasTransferenciasCuenta?> ObtenerTransferencia(int id);

        Task<bool> RegistrarIngresoManual(
            DateTime fecha,
            int idMoneda,
            int idCuenta,
            string concepto,
            decimal importe,
            int idUsuario);

        Task<bool> RegistrarEgresoManual(
            DateTime fecha,
            int idMoneda,
            int idCuenta,
            string concepto,
            decimal importe,
            int idUsuario);

        Task<bool> ActualizarMovimientoManual(
            int id,
            DateTime fecha,
            int idMoneda,
            int idCuenta,
            string concepto,
            decimal importe,
            int idUsuario);

        Task<bool> RegistrarTransferencia(
            DateTime fecha,
            int idMonedaOrigen,
            int idCuentaOrigen,
            decimal importeOrigen,
            int idMonedaDestino,
            int idCuentaDestino,
            decimal importeDestino,
            decimal cotizacion,
            string notaInterna,
            int idUsuario);

        Task<bool> ActualizarTransferencia(
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
            int idUsuario);

        Task<bool> Eliminar(int id, int idUsuario);
    }
}