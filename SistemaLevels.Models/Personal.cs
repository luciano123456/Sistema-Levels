using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class Personal
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Dni { get; set; }

    public int IdPais { get; set; }

    public int? IdTipoDocumento { get; set; }

    public string? NumeroDocumento { get; set; }

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public int? IdCondicionIva { get; set; }

    public virtual ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();

    public virtual PaisesCondicionesIva? IdCondicionIvaNavigation { get; set; }

    public virtual Pais IdPaisNavigation { get; set; } = null!;

    public virtual PaisesTiposDocumento? IdTipoDocumentoNavigation { get; set; }

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;

    public virtual ICollection<PersonalArtistasAsignado> PersonalArtistasAsignados { get; set; } = new List<PersonalArtistasAsignado>();

    public virtual ICollection<PersonalCuentaCorriente> PersonalCuentaCorrientes { get; set; } = new List<PersonalCuentaCorriente>();

    public virtual ICollection<PersonalPago> PersonalPagos { get; set; } = new List<PersonalPago>();

    public virtual ICollection<PersonalRolesAsignado> PersonalRolesAsignados { get; set; } = new List<PersonalRolesAsignado>();

    public virtual ICollection<PersonalSueldo> PersonalSueldos { get; set; } = new List<PersonalSueldo>();

    public virtual ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();

    public virtual ICollection<VentasCobrosComisione> VentasCobrosComisiones { get; set; } = new List<VentasCobrosComisione>();

    public virtual ICollection<VentasPersonal> VentasPersonals { get; set; } = new List<VentasPersonal>();
}
