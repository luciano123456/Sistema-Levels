using System;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMGastosFiltro
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }

        public int? IdCategoria { get; set; }
        public int? IdMoneda { get; set; }
        public int? IdCuenta { get; set; }
        public int? IdPersonal { get; set; }

        public string? Concepto { get; set; }
        public decimal? ImporteMin { get; set; }
    }
}
