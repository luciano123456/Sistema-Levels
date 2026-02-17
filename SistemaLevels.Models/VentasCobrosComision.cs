using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class VentasCobrosComisione
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public int IdVentaCobro { get; set; }

    public int? IdPersonalCc { get; set; }

    public int IdPersonal { get; set; }

    public decimal TotalComision { get; set; }

    public virtual Personal IdPersonalNavigation { get; set; } = null!;

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;

    public virtual VentasCobro IdVentaCobroNavigation { get; set; } = null!;
}
