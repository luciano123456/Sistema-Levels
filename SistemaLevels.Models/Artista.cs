using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class Artista
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public string Nombre { get; set; } = null!;

    public string NombreArtistico { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? TelefonoAlternativo { get; set; }

    public string? Dni { get; set; }

    public int IdPais { get; set; }

    public int? IdTipoDocumento { get; set; }

    public string? NumeroDocumento { get; set; }

    public string? Email { get; set; }

    public int IdProductora { get; set; }

    public int IdProvincia { get; set; }

    public string? Localidad { get; set; }

    public string? EntreCalles { get; set; }

    public string? Direccion { get; set; }

    public string? CodigoPostal { get; set; }

    public int? IdCondicionIva { get; set; }

    public int IdRepresentante { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public int IdMoneda { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal PrecioNegMax { get; set; }

    public decimal PrecioNegMin { get; set; }

    public virtual ICollection<ArtistasCuentaCorriente> ArtistasCuentaCorrientes { get; set; } = new List<ArtistasCuentaCorriente>();

    public virtual ICollection<ArtistasPago> ArtistasPagos { get; set; } = new List<ArtistasPago>();

    public virtual PaisesCondicionesIva? IdCondicionIvaNavigation { get; set; }

    public virtual PaisesMoneda IdMonedaNavigation { get; set; } = null!;

    public virtual Pais IdPaisNavigation { get; set; } = null!;

    public virtual Productora IdProductoraNavigation { get; set; } = null!;

    public virtual PaisesProvincia IdProvinciaNavigation { get; set; } = null!;

    public virtual Representante IdRepresentanteNavigation { get; set; } = null!;

    public virtual PaisesTiposDocumento? IdTipoDocumentoNavigation { get; set; }

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;

    public virtual ICollection<PersonalArtistasAsignado> PersonalArtistasAsignados { get; set; } = new List<PersonalArtistasAsignado>();

    public virtual ICollection<PresupuestosDetalle> PresupuestosDetalles { get; set; } = new List<PresupuestosDetalle>();

    public virtual ICollection<VentasArtista> VentasArtista { get; set; } = new List<VentasArtista>();
}
