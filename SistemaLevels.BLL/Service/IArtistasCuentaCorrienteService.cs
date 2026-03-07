using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IArtistasCuentaCorrienteService
    {
        Task<List<(Artista artista, decimal saldo)>> ListarArtistas(string? buscar);

        Task<(ArtistasCuentaCorriente? mov, string? cuenta, decimal saldo)> ObtenerMovimiento(int id);

        Task<List<ArtistasCuentaCorriente>> Movimientos(
            int idArtista,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto);

        Task<decimal> SaldoAnterior(
            int idArtista,
            int? idMoneda,
            DateTime? desde);

        Task<(decimal debe, decimal haber, int cantidad)> Resumen(
            int idArtista,
            int? idMoneda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoMov,
            string? texto);

        Task<bool> RegistrarPago(
            int idArtista,
            int idMoneda,
            int idCuenta,
            DateTime fecha,
            string concepto,
            decimal importe,
            int idUsuario);

        Task<bool> RegistrarAjuste(
            int idArtista,
            int idMoneda,
            DateTime fecha,
            string concepto,
            decimal debe,
            decimal haber,
            int idUsuario);

        Task<bool> Eliminar(int id);
    }
}