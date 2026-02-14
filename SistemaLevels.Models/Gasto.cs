using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class Gasto
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public DateTime Fecha { get; set; }

    public int IdCategoria { get; set; }

    public int IdMoneda { get; set; }

    public int IdCuenta { get; set; }

    public int? IdPersonal { get; set; }

    public string Concepto { get; set; } = null!;

    public decimal Importe { get; set; }

    public string? NotaInterna { get; set; }

    public virtual GastosCategoria IdCategoriaNavigation { get; set; } = null!;

    public virtual MonedasCuenta IdCuentaNavigation { get; set; } = null!;

    public virtual PaisesMoneda IdMonedaNavigation { get; set; } = null!;

    public virtual Personal? IdPersonalNavigation { get; set; }

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;
}
