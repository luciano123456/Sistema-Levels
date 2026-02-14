using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class VentasArtista
{
    public int Id { get; set; }

    public int IdArtistaCc { get; set; }

    public int IdVenta { get; set; }

    public int IdArtista { get; set; }

    public int IdRepresentante { get; set; }

    public decimal PorcComision { get; set; }

    public decimal TotalComision { get; set; }

    public virtual Artista IdArtistaNavigation { get; set; } = null!;

    public virtual Representante IdRepresentanteNavigation { get; set; } = null!;

    public virtual Venta IdVentaNavigation { get; set; } = null!;
}
