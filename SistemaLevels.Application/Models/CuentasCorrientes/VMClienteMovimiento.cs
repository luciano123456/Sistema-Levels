using System;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMClientesCuentaCorrienteFiltro
    {
        public int? IdCliente { get; set; }

        public int? IdMoneda { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        public string? TipoMov { get; set; }

        public string? Texto { get; set; }

        public bool SoloSaldoActivo { get; set; }

        public string? BuscarCliente { get; set; }
    }

    public class VMClientesCuentaCorrienteCliente
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

    public class VMClientesCuentaCorrienteMovimiento
    {
        public int Id { get; set; }

        public int IdCliente { get; set; }

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

    public class VMClientesCuentaCorrienteCobro
    {
        public int IdCliente { get; set; }

        public int IdMoneda { get; set; }

        public int IdCuenta { get; set; }

        public DateTime Fecha { get; set; }

        public string Concepto { get; set; } = "";

        public decimal Importe { get; set; }
    }

    public class VMClientesCuentaCorrienteAjuste
    {
        public int IdCliente { get; set; }

        public int IdMoneda { get; set; }

        public DateTime Fecha { get; set; }

        public string Concepto { get; set; } = "";

        public decimal Debe { get; set; }

        public decimal Haber { get; set; }
    }

    public class VMClientesCuentaCorrienteResumen
    {
        public decimal SaldoAnterior { get; set; }

        public decimal Debe { get; set; }

        public decimal Haber { get; set; }

        public decimal SaldoActual { get; set; }

        public int CantidadMovimientos { get; set; }
    }
}