using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class CajasTransferenciasCuenta
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public DateTime Fecha { get; set; }

    public int IdMonedaOrigen { get; set; }

    public int IdCuentaOrigen { get; set; }

    public decimal ImporteOrigen { get; set; }

    public int IdMonedaDestino { get; set; }

    public int IdCuentaDestino { get; set; }

    public decimal ImporteDestino { get; set; }

    public decimal Cotizacion { get; set; }

    public string NotaInterna { get; set; } = null!;

    public virtual MonedasCuenta IdCuentaDestinoNavigation { get; set; } = null!;

    public virtual MonedasCuenta IdCuentaOrigenNavigation { get; set; } = null!;

    public virtual PaisesMoneda IdMonedaDestinoNavigation { get; set; } = null!;

    public virtual PaisesMoneda IdMonedaOrigenNavigation { get; set; } = null!;

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;
}
