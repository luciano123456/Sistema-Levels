using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class Representante
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public string Nombre { get; set; } = null!;

    public int IdPais { get; set; }

    public int? IdTipoDocumento { get; set; }

    public string? NumeroDocumento { get; set; }

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Dni { get; set; }

    public virtual ICollection<Artista> Artista { get; set; } = new List<Artista>();

    public virtual Pais IdPaisNavigation { get; set; } = null!;

    public virtual PaisesTiposDocumento? IdTipoDocumentoNavigation { get; set; }

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;

    public virtual ICollection<VentasArtista> VentasArtista { get; set; } = new List<VentasArtista>();
}
