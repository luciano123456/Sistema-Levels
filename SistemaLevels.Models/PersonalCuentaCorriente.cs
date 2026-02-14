using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class PersonalCuentaCorriente
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public int IdPersonal { get; set; }

    public int IdMoneda { get; set; }

    public string TipoMov { get; set; } = null!;

    public int? IdMov { get; set; }

    public DateTime Fecha { get; set; }

    public string Concepto { get; set; } = null!;

    public decimal Debe { get; set; }

    public decimal Haber { get; set; }

    public virtual PaisesMoneda IdMonedaNavigation { get; set; } = null!;

    public virtual Personal IdPersonalNavigation { get; set; } = null!;

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;
}
