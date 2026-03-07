using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class ClientesCuentaCorrienteService : IClientesCuentaCorrienteService
    {
        private readonly IClientesCuentaCorrienteRepository _repo;

        public ClientesCuentaCorrienteService(IClientesCuentaCorrienteRepository repo)
        {
            _repo = repo;
        }

        public Task<List<(Cliente cliente, decimal saldo)>> ListarClientes(string? buscar)
            => _repo.ListarClientes(buscar);

        public async Task<(ClientesCuentaCorriente? mov, string? cuenta, decimal saldo)> ObtenerMovimiento(int id)
        {
            return await _repo.ObtenerMovimiento(id);
        }

        public Task<List<ClientesCuentaCorriente>> Movimientos(
            int idCliente,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto)
            => _repo.Movimientos(idCliente, idMoneda, desde, hasta, tipoMov, texto);

        public Task<decimal> SaldoAnterior(
            int idCliente,
            int? idMoneda,
            DateTime? desde)
            => _repo.SaldoAnterior(idCliente, idMoneda, desde);

        public Task<(decimal debe, decimal haber, int cantidad)> Resumen(
            int idCliente,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto)
            => _repo.Resumen(idCliente, idMoneda, desde, hasta, tipoMov, texto);

        public Task<bool> RegistrarCobro(
            int idCliente,
            int idMoneda,
            int idCuenta,
            DateTime fecha,
            string concepto,
            decimal importe,
            int idUsuario)
            => _repo.RegistrarCobro(idCliente, idMoneda, idCuenta, fecha, concepto, importe, idUsuario);

        public Task<bool> RegistrarAjuste(
            int idCliente,
            int idMoneda,
            DateTime fecha,
            string concepto,
            decimal debe,
            decimal haber,
            int idUsuario)
            => _repo.RegistrarAjuste(idCliente, idMoneda, fecha, concepto, debe, haber, idUsuario);

        public Task<bool> Eliminar(int id)
            => _repo.Eliminar(id);
    }
}