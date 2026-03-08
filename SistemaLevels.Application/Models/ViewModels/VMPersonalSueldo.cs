using System;
using System.Collections.Generic;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMPersonalSueldo
    {
        public int Id { get; set; }

        public int IdUsuarioRegistra { get; set; }
        public DateTime FechaRegistra { get; set; }

        public int? IdUsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }

        public int? IdPersonalCc { get; set; }

        public DateTime Fecha { get; set; }

        public int IdPersonal { get; set; }
        public string? Personal { get; set; }

        public int IdMoneda { get; set; }
        public string? Moneda { get; set; }

        public string Concepto { get; set; } = "";

        public decimal ImporteTotal { get; set; }
        public decimal ImportePagado { get; set; }
        public decimal Saldo { get; set; }

        public string Estado { get; set; } = "";

        public string? NotaPersonal { get; set; }
        public string? NotaInterna { get; set; }

        public string? UsuarioRegistra { get; set; }
        public string? UsuarioModifica { get; set; }

        public List<VMPersonalSueldoPago> Pagos { get; set; } = new();
    }

    public class VMPersonalSueldoPago
    {
        public int Id { get; set; }

        public int IdUsuarioRegistra { get; set; }
        public DateTime FechaRegistra { get; set; }

        public int? IdUsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }

        public int IdSueldo { get; set; }

        public int? IdPersonalCc { get; set; }
        public int? IdCaja { get; set; }

        public DateTime Fecha { get; set; }

        public int IdMoneda { get; set; }
        public string? Moneda { get; set; }

        public int IdCuenta { get; set; }
        public string? Cuenta { get; set; }

        public decimal Importe { get; set; }
        public decimal Cotizacion { get; set; }
        public decimal Conversion { get; set; }
    }

    public class VMPersonalSueldosFiltro
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? IdPersonal { get; set; }
        public int? IdMoneda { get; set; }
        public string? Estado { get; set; }
    }
}