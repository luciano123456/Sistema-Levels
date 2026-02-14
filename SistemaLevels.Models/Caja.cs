using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class Caja
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public string TipoMov { get; set; } = null!;

    public int? IdMov { get; set; }

    public DateTime Fecha { get; set; }

    public string Concepto { get; set; } = null!;

    public int IdMoneda { get; set; }

    public int IdCuenta { get; set; }

    public decimal Ingreso { get; set; }

    public decimal Egrreso { get; set; }

    public decimal Saldo { get; set; }

    public virtual MonedasCuenta IdCuentaNavigation { get; set; } = null!;

    public virtual PaisesMoneda IdMonedaNavigation { get; set; } = null!;

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;
}
