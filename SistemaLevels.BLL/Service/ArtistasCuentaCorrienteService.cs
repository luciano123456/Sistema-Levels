using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class ArtistasCuentaCorrienteService : IArtistasCuentaCorrienteService
    {
        private readonly IArtistasCuentaCorrienteRepository _repo;

        public ArtistasCuentaCorrienteService(IArtistasCuentaCorrienteRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<(Artista artista, decimal saldo)>> ListarArtistas(string? buscar)
        {
            return await _repo.ListarArtistas(buscar);
        }

        public async Task<(ArtistasCuentaCorriente? mov, string? cuenta, decimal saldo)> ObtenerMovimiento(int id)
        {
            return await _repo.ObtenerMovimiento(id);
        }

        public Task<List<ArtistasCuentaCorriente>> Movimientos(
            int idArtista,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto)
            => _repo.Movimientos(idArtista, idMoneda, desde, hasta, tipoMov, texto);

        public Task<decimal> SaldoAnterior(
            int idArtista,
            int? idMoneda,
            DateTime? desde)
            => _repo.SaldoAnterior(idArtista, idMoneda, desde);

        public Task<(decimal debe, decimal haber, int cantidad)> Resumen(
            int idArtista,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto)
            => _repo.Resumen(idArtista, idMoneda, desde, hasta, tipoMov, texto);

        public Task<bool> RegistrarPago(
            int idArtista,
            int idMoneda,
            int idCuenta,
            DateTime fecha,
            string concepto,
            decimal importe,
            int idUsuario)
            => _repo.RegistrarPago(idArtista, idMoneda, idCuenta, fecha, concepto, importe, idUsuario);

        public Task<bool> RegistrarAjuste(
            int idArtista,
            int idMoneda,
            DateTime fecha,
            string concepto,
            decimal debe,
            decimal haber,
            int idUsuario)
            => _repo.RegistrarAjuste(idArtista, idMoneda, fecha, concepto, debe, haber, idUsuario);

        public Task<bool> Eliminar(int id)
            => _repo.Eliminar(id);
    }
}