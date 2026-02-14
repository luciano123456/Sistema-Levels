using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class PersonalPago
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public int? IdPersonalCc { get; set; }

    public int? IdMonedaCc { get; set; }

    public int? IdCaja { get; set; }

    public int IdPersonal { get; set; }

    public DateTime Fecha { get; set; }

    public string Concepto { get; set; } = null!;

    public int IdMoneda { get; set; }

    public int IdCuenta { get; set; }

    public decimal Importe { get; set; }

    public decimal Cotizacion { get; set; }

    public decimal Conversion { get; set; }

    public string? NotaPersonal { get; set; }

    public string? NotaInterna { get; set; }

    public virtual MonedasCuenta IdCuentaNavigation { get; set; } = null!;

    public virtual PaisesMoneda IdMonedaNavigation { get; set; } = null!;

    public virtual Personal IdPersonalNavigation { get; set; } = null!;

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;
}
