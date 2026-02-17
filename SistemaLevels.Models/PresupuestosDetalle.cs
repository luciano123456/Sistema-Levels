using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class PresupuestosDetalle
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public int IdPresupuesto { get; set; }

    public int? IdVenta { get; set; }

    public DateTime FechaEvento { get; set; }

    public int IdArtista { get; set; }

    public int IdUbicacion { get; set; }

    public string NombreEvento { get; set; } = null!;

    public DateTime Duracion { get; set; }

    public int CantidadPersonas { get; set; }

    public int IdMoneda { get; set; }

    public decimal Importe { get; set; }

    public string? NotaCliente { get; set; }

    public string? NotaInterna { get; set; }

    public virtual Artista IdArtistaNavigation { get; set; } = null!;

    public virtual PaisesMoneda IdMonedaNavigation { get; set; } = null!;

    public virtual Presupuesto IdPresupuestoNavigation { get; set; } = null!;

    public virtual Ubicacion IdUbicacionNavigation { get; set; } = null!;

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;
}
