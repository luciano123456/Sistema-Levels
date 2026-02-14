using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class VentasPersonal
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public int IdVenta { get; set; }

    public int IdPersonal { get; set; }

    public int IdCargo { get; set; }

    public int IdTipoComision { get; set; }

    public decimal PorcComision { get; set; }

    public decimal TotalComision { get; set; }

    public virtual PersonalCargo IdCargoNavigation { get; set; } = null!;

    public virtual Personal IdPersonalNavigation { get; set; } = null!;

    public virtual TiposComisione IdTipoComisionNavigation { get; set; } = null!;

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;

    public virtual Venta IdVentaNavigation { get; set; } = null!;
}
