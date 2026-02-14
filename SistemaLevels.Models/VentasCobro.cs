using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class VentasCobro
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public int IdVenta { get; set; }

    public int? IdClienteCc { get; set; }

    public int? IdArtistaCc { get; set; }

    public int? IdCaja { get; set; }

    public DateTime Fecha { get; set; }

    public int IdMoneda { get; set; }

    public int IdCuenta { get; set; }

    public decimal Importe { get; set; }

    public decimal Cotizacion { get; set; }

    public decimal Conversion { get; set; }

    public string? NotaCliente { get; set; }

    public string? NotaInterna { get; set; }

    public virtual MonedasCuenta IdCuentaNavigation { get; set; } = null!;

    public virtual PaisesMoneda IdMonedaNavigation { get; set; } = null!;

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;

    public virtual Venta IdVentaNavigation { get; set; } = null!;

    public virtual ICollection<VentasCobrosComisione> VentasCobrosComisiones { get; set; } = new List<VentasCobrosComisione>();
}
