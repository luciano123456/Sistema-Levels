using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class User
{
    public int Id { get; set; }

    public string Usuario { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Apellido { get; set; }

    public string? Dni { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public int IdRol { get; set; }

    public string Contrasena { get; set; } = null!;

    public int IdEstado { get; set; }

    public string? Correo { get; set; }

    public string? CodigoRecuperacion { get; set; }

    public virtual ICollection<Artista> ArtistaIdUsuarioModificaNavigations { get; set; } = new List<Artista>();

    public virtual ICollection<Artista> ArtistaIdUsuarioRegistraNavigations { get; set; } = new List<Artista>();

    public virtual ICollection<ArtistasCuentaCorriente> ArtistasCuentaCorrienteIdUsuarioModificaNavigations { get; set; } = new List<ArtistasCuentaCorriente>();

    public virtual ICollection<ArtistasCuentaCorriente> ArtistasCuentaCorrienteIdUsuarioRegistraNavigations { get; set; } = new List<ArtistasCuentaCorriente>();

    public virtual ICollection<ArtistasPago> ArtistasPagoIdUsuarioModificaNavigations { get; set; } = new List<ArtistasPago>();

    public virtual ICollection<ArtistasPago> ArtistasPagoIdUsuarioRegistraNavigations { get; set; } = new List<ArtistasPago>();

    public virtual ICollection<Caja> CajaIdUsuarioModificaNavigations { get; set; } = new List<Caja>();

    public virtual ICollection<Caja> CajaIdUsuarioRegistraNavigations { get; set; } = new List<Caja>();

    public virtual ICollection<CajasTransferenciasCuenta> CajasTransferenciasCuentaIdUsuarioModificaNavigations { get; set; } = new List<CajasTransferenciasCuenta>();

    public virtual ICollection<CajasTransferenciasCuenta> CajasTransferenciasCuentaIdUsuarioRegistraNavigations { get; set; } = new List<CajasTransferenciasCuenta>();

    public virtual ICollection<Cliente> ClienteIdUsuarioModificaNavigations { get; set; } = new List<Cliente>();

    public virtual ICollection<Cliente> ClienteIdUsuarioRegistraNavigations { get; set; } = new List<Cliente>();

    public virtual ICollection<ClientesCuentaCorriente> ClientesCuentaCorrienteIdUsuarioModificaNavigations { get; set; } = new List<ClientesCuentaCorriente>();

    public virtual ICollection<ClientesCuentaCorriente> ClientesCuentaCorrienteIdUsuarioRegistraNavigations { get; set; } = new List<ClientesCuentaCorriente>();

    public virtual ICollection<Gasto> GastoIdUsuarioModificaNavigations { get; set; } = new List<Gasto>();

    public virtual ICollection<Gasto> GastoIdUsuarioRegistraNavigations { get; set; } = new List<Gasto>();

    public virtual UsuariosEstado IdEstadoNavigation { get; set; } = null!;

    public virtual UsuariosRole IdRolNavigation { get; set; } = null!;

    public virtual ICollection<PersonalCuentaCorriente> PersonalCuentaCorrienteIdUsuarioModificaNavigations { get; set; } = new List<PersonalCuentaCorriente>();

    public virtual ICollection<PersonalCuentaCorriente> PersonalCuentaCorrienteIdUsuarioRegistraNavigations { get; set; } = new List<PersonalCuentaCorriente>();

    public virtual ICollection<Personal> PersonalIdUsuarioModificaNavigations { get; set; } = new List<Personal>();

    public virtual ICollection<Personal> PersonalIdUsuarioRegistraNavigations { get; set; } = new List<Personal>();

    public virtual ICollection<PersonalPago> PersonalPagoIdUsuarioModificaNavigations { get; set; } = new List<PersonalPago>();

    public virtual ICollection<PersonalPago> PersonalPagoIdUsuarioRegistraNavigations { get; set; } = new List<PersonalPago>();

    public virtual ICollection<PersonalSueldo> PersonalSueldoIdUsuarioModificaNavigations { get; set; } = new List<PersonalSueldo>();

    public virtual ICollection<PersonalSueldo> PersonalSueldoIdUsuarioRegistraNavigations { get; set; } = new List<PersonalSueldo>();

    public virtual ICollection<PersonalSueldosPago> PersonalSueldosPagoIdUsuarioModificaNavigations { get; set; } = new List<PersonalSueldosPago>();

    public virtual ICollection<PersonalSueldosPago> PersonalSueldosPagoIdUsuarioRegistraNavigations { get; set; } = new List<PersonalSueldosPago>();

    public virtual ICollection<Presupuesto> PresupuestoIdUsuarioModificaNavigations { get; set; } = new List<Presupuesto>();

    public virtual ICollection<Presupuesto> PresupuestoIdUsuarioRegistraNavigations { get; set; } = new List<Presupuesto>();

    public virtual ICollection<PresupuestosDetalle> PresupuestosDetalleIdUsuarioModificaNavigations { get; set; } = new List<PresupuestosDetalle>();

    public virtual ICollection<PresupuestosDetalle> PresupuestosDetalleIdUsuarioRegistraNavigations { get; set; } = new List<PresupuestosDetalle>();

    public virtual ICollection<Productora> ProductoraIdUsuarioModificaNavigations { get; set; } = new List<Productora>();

    public virtual ICollection<Productora> ProductoraIdUsuarioRegistraNavigations { get; set; } = new List<Productora>();

    public virtual ICollection<Representante> RepresentanteIdUsuarioModificaNavigations { get; set; } = new List<Representante>();

    public virtual ICollection<Representante> RepresentanteIdUsuarioRegistraNavigations { get; set; } = new List<Representante>();

    public virtual ICollection<Tarea> TareaIdUsuarioModificaNavigations { get; set; } = new List<Tarea>();

    public virtual ICollection<Tarea> TareaIdUsuarioRegistraNavigations { get; set; } = new List<Tarea>();

    public virtual ICollection<Venta> VentaIdUsuarioModificaNavigations { get; set; } = new List<Venta>();

    public virtual ICollection<Venta> VentaIdUsuarioRegistraNavigations { get; set; } = new List<Venta>();

    public virtual ICollection<VentasCobro> VentasCobroIdUsuarioModificaNavigations { get; set; } = new List<VentasCobro>();

    public virtual ICollection<VentasCobro> VentasCobroIdUsuarioRegistraNavigations { get; set; } = new List<VentasCobro>();

    public virtual ICollection<VentasCobrosComisione> VentasCobrosComisioneIdUsuarioModificaNavigations { get; set; } = new List<VentasCobrosComisione>();

    public virtual ICollection<VentasCobrosComisione> VentasCobrosComisioneIdUsuarioRegistraNavigations { get; set; } = new List<VentasCobrosComisione>();

    public virtual ICollection<VentasPersonal> VentasPersonalIdUsuarioModificaNavigations { get; set; } = new List<VentasPersonal>();

    public virtual ICollection<VentasPersonal> VentasPersonalIdUsuarioRegistraNavigations { get; set; } = new List<VentasPersonal>();
}
