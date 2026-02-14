using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class Presupuesto
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public DateTime Fecha { get; set; }

    public int IdCliente { get; set; }

    public int IdProductora { get; set; }

    public int? IdEstado { get; set; }

    public string? NotaCliente { get; set; }

    public string? NotaInterna { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual PresupuestosEstado? IdEstadoNavigation { get; set; }

    public virtual Productora IdProductoraNavigation { get; set; } = null!;

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;

    public virtual ICollection<PresupuestosDetalle> PresupuestosDetalles { get; set; } = new List<PresupuestosDetalle>();
}
