using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IPersonalCuentaCorrienteService
    {
        Task<List<(Personal personal, decimal saldo)>> ListarPersonal(string? buscar);

        Task<(PersonalCuentaCorriente? mov, string? cuenta, decimal saldo)> ObtenerMovimiento(int id);

        Task<List<PersonalCuentaCorriente>> Movimientos(
            int idPersonal,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto);

        Task<decimal> SaldoAnterior(
            int idPersonal,
            int? idMoneda,
            DateTime? desde);

        Task<(decimal debe, decimal haber, int cantidad)> Resumen(
            int idPersonal,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto);

        Task<bool> RegistrarPago(
            int idPersonal,
            int idMoneda,
            int idCuenta,
            DateTime fecha,
            string concepto,
            decimal importe,
            int idUsuario);

        Task<bool> RegistrarAjuste(
            int idPersonal,
            int idMoneda,
            DateTime fecha,
            string concepto,
            decimal debe,
            decimal haber,
            int idUsuario);

        Task<bool> Eliminar(int id);
    }
}