using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class PaisesMoneda
{
    public int Id { get; set; }

    public int IdPais { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Cotizacion { get; set; }

    public virtual ICollection<Artista> Artista { get; set; } = new List<Artista>();

    public virtual ICollection<ArtistasCuentaCorriente> ArtistasCuentaCorrientes { get; set; } = new List<ArtistasCuentaCorriente>();

    public virtual ICollection<ArtistasPago> ArtistasPagos { get; set; } = new List<ArtistasPago>();

    public virtual ICollection<Caja> Cajas { get; set; } = new List<Caja>();

    public virtual ICollection<CajasTransferenciasCuenta> CajasTransferenciasCuentaIdMonedaDestinoNavigations { get; set; } = new List<CajasTransferenciasCuenta>();

    public virtual ICollection<CajasTransferenciasCuenta> CajasTransferenciasCuentaIdMonedaOrigenNavigations { get; set; } = new List<CajasTransferenciasCuenta>();

    public virtual ICollection<ClientesCuentaCorriente> ClientesCuentaCorrientes { get; set; } = new List<ClientesCuentaCorriente>();

    public virtual ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();

    public virtual Pais IdPaisNavigation { get; set; } = null!;

    public virtual ICollection<MonedasCuenta> MonedasCuenta { get; set; } = new List<MonedasCuenta>();

    public virtual ICollection<PersonalCuentaCorriente> PersonalCuentaCorrientes { get; set; } = new List<PersonalCuentaCorriente>();

    public virtual ICollection<PersonalPago> PersonalPagos { get; set; } = new List<PersonalPago>();

    public virtual ICollection<PersonalSueldo> PersonalSueldos { get; set; } = new List<PersonalSueldo>();

    public virtual ICollection<PersonalSueldosPago> PersonalSueldosPagos { get; set; } = new List<PersonalSueldosPago>();

    public virtual ICollection<PresupuestosDetalle> PresupuestosDetalles { get; set; } = new List<PresupuestosDetalle>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();

    public virtual ICollection<VentasCobro> VentasCobros { get; set; } = new List<VentasCobro>();
}
