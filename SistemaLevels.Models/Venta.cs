using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class Venta
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public int? IdPresupuesto { get; set; }

    public int? IdClienteCc { get; set; }

    public DateTime Fecha { get; set; }

    public int IdUbicacion { get; set; }

    public string NombreEvento { get; set; } = null!;

    public DateTime Duracion { get; set; }

    public int IdCliente { get; set; }

    public int IdProductora { get; set; }

    public int IdMoneda { get; set; }

    public int IdEstado { get; set; }

    public decimal ImporteTotal { get; set; }

    public decimal ImporteAbonado { get; set; }

    public decimal Saldo { get; set; }

    public string? NotaInterna { get; set; }

    public string? NotaCliente { get; set; }

    public int IdTipoContrato { get; set; }

    public int? IdOpExclusividad { get; set; }

    public int? DiasPrevios { get; set; }

    public DateTime? FechaHasta { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual VentasEstado IdEstadoNavigation { get; set; } = null!;

    public virtual PaisesMoneda IdMonedaNavigation { get; set; } = null!;

    public virtual OpcionesBinaria? IdOpExclusividadNavigation { get; set; }

    public virtual Productora IdProductoraNavigation { get; set; } = null!;

    public virtual TiposContrato IdTipoContratoNavigation { get; set; } = null!;

    public virtual Ubicacione IdUbicacionNavigation { get; set; } = null!;

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;

    public virtual ICollection<VentasArtista> VentasArtista { get; set; } = new List<VentasArtista>();

    public virtual ICollection<VentasCobro> VentasCobros { get; set; } = new List<VentasCobro>();

    public virtual ICollection<VentasPersonal> VentasPersonals { get; set; } = new List<VentasPersonal>();
}
