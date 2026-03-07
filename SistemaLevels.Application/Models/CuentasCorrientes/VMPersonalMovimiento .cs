namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMPersonalCuentaCorrienteFiltro
    {
        public int? IdPersonal { get; set; }

        public int? IdMoneda { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        public string? TipoMov { get; set; }

        public string? Texto { get; set; }

        public bool SoloSaldoActivo { get; set; }

        public string? BuscarPersonal { get; set; }
    }

    public class VMPersonalCuentaCorrientePersonal
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public decimal Saldo { get; set; }
    }

    public class VMPersonalCuentaCorrienteMovimiento
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string TipoMov { get; set; } = "";

        public string Concepto { get; set; } = "";

        public decimal Debe { get; set; }

        public decimal Haber { get; set; }

        public decimal Saldo { get; set; }

        public string? Moneda { get; set; }

        public bool PuedeEliminar { get; set; }
    }

    public class VMPersonalCuentaCorrientePago
    {
        public int IdPersonal { get; set; }

        public int IdMoneda { get; set; }

        public int IdCuenta { get; set; }

        public DateTime Fecha { get; set; }

        public string Concepto { get; set; } = "";

        public decimal Importe { get; set; }
    }

    public class VMPersonalCuentaCorrienteAjuste
    {
        public int IdPersonal { get; set; }

        public int IdMoneda { get; set; }

        public DateTime Fecha { get; set; }

        public string Concepto { get; set; } = "";

        public decimal Debe { get; set; }

        public decimal Haber { get; set; }
    }

    public class VMPersonalCuentaCorrienteResumen
    {
        public decimal SaldoAnterior { get; set; }

        public decimal Debe { get; set; }

        public decimal Haber { get; set; }

        public decimal SaldoActual { get; set; }

        public int CantidadMovimientos { get; set; }
    }
}