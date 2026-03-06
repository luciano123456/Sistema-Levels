using System;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMArtistasCuentaCorrienteFiltro
    {
        public int? IdArtista { get; set; }

        public int? IdMoneda { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        public string? TipoMov { get; set; }

        public string? Texto { get; set; }

        public bool SoloSaldoActivo { get; set; }

        public string? BuscarArtista { get; set; }
    }

    public class VMArtistasCuentaCorrienteArtista
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public int? IdMoneda { get; set; }

        public string? Moneda { get; set; }

        public string? SimboloMoneda { get; set; }

        public decimal Debe { get; set; }

        public decimal Haber { get; set; }

        public decimal Saldo { get; set; }
    }

    public class VMArtistasCuentaCorrienteMovimiento
    {
        public int Id { get; set; }

        public int IdArtista { get; set; }

        public int IdMoneda { get; set; }

        public DateTime Fecha { get; set; }

        public string TipoMov { get; set; } = "";

        public int? IdMov { get; set; }

        public string Concepto { get; set; } = "";

        public decimal Debe { get; set; }

        public decimal Haber { get; set; }

        public decimal Saldo { get; set; }

        public string? Moneda { get; set; }

        public string? SimboloMoneda { get; set; }

        public bool PuedeEliminar { get; set; }
    }

    public class VMArtistasCuentaCorrientePago
    {
        public int IdArtista { get; set; }

        public int IdMoneda { get; set; }
        public int IdCuenta { get; set; }

        public DateTime Fecha { get; set; }

        public string Concepto { get; set; } = "";

        public decimal Importe { get; set; }
    }

    public class VMArtistasCuentaCorrienteAjuste
    {
        public int IdArtista { get; set; }

        public int IdMoneda { get; set; }

        public DateTime Fecha { get; set; }

        public string Concepto { get; set; } = "";

        public decimal Debe { get; set; }

        public decimal Haber { get; set; }
    }

    public class VMArtistasCuentaCorrienteResumen
    {
        public decimal SaldoAnterior { get; set; }

        public decimal Debe { get; set; }

        public decimal Haber { get; set; }

        public decimal SaldoActual { get; set; }

        public int CantidadMovimientos { get; set; }
    }
}