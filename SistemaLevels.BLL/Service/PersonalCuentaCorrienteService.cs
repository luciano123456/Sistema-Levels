using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class PersonalCuentaCorrienteService : IPersonalCuentaCorrienteService
    {
        private readonly IPersonalCuentaCorrienteRepository _repo;

        public PersonalCuentaCorrienteService(IPersonalCuentaCorrienteRepository repo)
        {
            _repo = repo;
        }

        public Task<List<(Personal personal, decimal saldo)>> ListarPersonal(string? buscar)
            => _repo.ListarPersonal(buscar);

        public Task<(PersonalCuentaCorriente? mov, string? cuenta, decimal saldo)> ObtenerMovimiento(int id)
            => _repo.ObtenerMovimiento(id);

        public Task<List<PersonalCuentaCorriente>> Movimientos(
            int idPersonal,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto)
            => _repo.Movimientos(idPersonal, idMoneda, desde, hasta, tipoMov, texto);

        public Task<decimal> SaldoAnterior(int idPersonal, int? idMoneda, DateTime? desde)
            => _repo.SaldoAnterior(idPersonal, idMoneda, desde);

        public Task<(decimal debe, decimal haber, int cantidad)> Resumen(
            int idPersonal,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto)
            => _repo.Resumen(idPersonal, idMoneda, desde, hasta, tipoMov, texto);

        public Task<bool> RegistrarPago(
            int idPersonal,
            int idMoneda,
            int idCuenta,
            DateTime fecha,
            string concepto,
            decimal importe,
            int idUsuario)
            => _repo.RegistrarPago(idPersonal, idMoneda, idCuenta, fecha, concepto, importe, idUsuario);

        public Task<bool> RegistrarAjuste(
            int idPersonal,
            int idMoneda,
            DateTime fecha,
            string concepto,
            decimal debe,
            decimal haber,
            int idUsuario)
            => _repo.RegistrarAjuste(idPersonal, idMoneda, fecha, concepto, debe, haber, idUsuario);

        public Task<bool> Eliminar(int id)
            => _repo.Eliminar(id);
    }
}