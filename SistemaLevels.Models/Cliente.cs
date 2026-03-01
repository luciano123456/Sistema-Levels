using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class Cliente
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? TelefonoAlternativo { get; set; }

    public string? Dni { get; set; }

    public int? IdTipoDocumento { get; set; }

    public string? NumeroDocumento { get; set; }

    public string? Email { get; set; }

    public int? IdPais { get; set; }

    public int? IdProvincia { get; set; }

    public string? Localidad { get; set; }

    public string? EntreCalles { get; set; }

    public string? Direccion { get; set; }

    public string? CodigoPostal { get; set; }

    public int? IdCondicionIva { get; set; }

    public int? AsociacionAutomatica { get; set; }

    public virtual ICollection<ClientesCuentaCorriente> ClientesCuentaCorrientes { get; set; } = new List<ClientesCuentaCorriente>();

    public virtual ICollection<ClientesProductora> ClientesProductoras { get; set; } = new List<ClientesProductora>();

    public virtual PaisesCondicionesIva? IdCondicionIvaNavigation { get; set; }

    public virtual Pais IdPaisNavigation { get; set; } = null!;

    public virtual PaisesProvincia IdProvinciaNavigation { get; set; } = null!;

    public virtual PaisesTiposDocumento? IdTipoDocumentoNavigation { get; set; }

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;

    public virtual ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
