using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class MonedasCuenta
{
    public int Id { get; set; }

    public int? IdMoneda { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<ArtistasPago> ArtistasPagos { get; set; } = new List<ArtistasPago>();

    public virtual ICollection<Caja> Cajas { get; set; } = new List<Caja>();

    public virtual ICollection<CajasTransferenciasCuenta> CajasTransferenciasCuentaIdCuentaDestinoNavigations { get; set; } = new List<CajasTransferenciasCuenta>();

    public virtual ICollection<CajasTransferenciasCuenta> CajasTransferenciasCuentaIdCuentaOrigenNavigations { get; set; } = new List<CajasTransferenciasCuenta>();

    public virtual ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();

    public virtual PaisesMoneda? IdMonedaNavigation { get; set; }


    public virtual ICollection<PersonalPago> PersonalPagos { get; set; } = new List<PersonalPago>();

    public virtual ICollection<PersonalSueldosPago> PersonalSueldosPagos { get; set; } = new List<PersonalSueldosPago>();

    public virtual ICollection<VentasCobro> VentasCobros { get; set; } = new List<VentasCobro>();
}
