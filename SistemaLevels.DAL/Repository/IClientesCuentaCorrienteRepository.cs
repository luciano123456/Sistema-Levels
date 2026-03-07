using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public interface IClientesCuentaCorrienteRepository
    {
        Task<List<(Cliente cliente, decimal saldo)>> ListarClientes(string? buscar);

        Task<(ClientesCuentaCorriente? mov, string? cuenta, decimal saldo)> ObtenerMovimiento(int id);

        Task<List<ClientesCuentaCorriente>> Movimientos(
            int idCliente,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto);

        Task<decimal> SaldoAnterior(
            int idCliente,
            int? idMoneda,
            DateTime? desde);

        Task<(decimal debe, decimal haber, int cantidad)> Resumen(
            int idCliente,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto);

        Task<bool> RegistrarCobro(
            int idCliente,
            int idMoneda,
            int idCuenta,
            DateTime fecha,
            string concepto,
            decimal importe,
            int idUsuario);

        Task<bool> RegistrarAjuste(
            int idCliente,
            int idMoneda,
            DateTime fecha,
            string concepto,
            decimal debe,
            decimal haber,
            int idUsuario);

        Task<bool> Eliminar(int id);
    }
}