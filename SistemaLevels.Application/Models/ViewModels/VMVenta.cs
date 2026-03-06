using System;
using System.Collections.Generic;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMVenta
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }
        public int IdUbicacion { get; set; }
        public string NombreEvento { get; set; } = "";
        public DateTime Duracion { get; set; }

        public int IdCliente { get; set; }
        public int IdProductora { get; set; }
        public int IdMoneda { get; set; }
        public int IdEstado { get; set; }

        public decimal ImporteTotal { get; set; }
        public decimal ImporteAbonado { get; set; } // se recalcula desde cobros
        public decimal Saldo { get; set; }          // se recalcula desde cobros

        public string? NotaInterna { get; set; }
        public string? NotaCliente { get; set; }

        public int IdTipoContrato { get; set; }
        public int? IdOpExclusividad { get; set; }
        public int? DiasPrevios { get; set; }
        public DateTime? FechaHasta { get; set; }

        // CC (se deja null)
        public int? IdPresupuesto { get; set; }
        public int? IdClienteCc { get; set; }

        // textos (para grilla)
        public string? Cliente { get; set; }
        public string? Productora { get; set; }
        public string? Moneda { get; set; }
        public string? Estado { get; set; }
        public string? Ubicacion { get; set; }
        public string? TipoContrato { get; set; }
        public string? Exclusividad { get; set; }

        // Detalles
        public List<VMVentaArtista> Artistas { get; set; } = new();
        public List<VMVentaPersonal> Personal { get; set; } = new();
        public List<VMVentaCobro> Cobros { get; set; } = new();

        // Auditoría
        public int IdUsuarioRegistra { get; set; }
        public DateTime FechaRegistra { get; set; }
        public string? UsuarioRegistra { get; set; }

        public int? IdUsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }
        public string? UsuarioModifica { get; set; }
    }

    public class VMVentaArtista
    {
        public int Id { get; set; }
        public int IdArtista { get; set; }
        public int IdRepresentante { get; set; }
        public decimal PorcComision { get; set; }
        public decimal TotalComision { get; set; }

        public int? IdArtistaCc { get; set; }

        public string? Artista { get; set; }
        public string? Representante { get; set; }
    }
    public class VMVentaPersonal
    {
        public int Id { get; set; }
        public int IdPersonal { get; set; }
        public int IdCargo { get; set; }
        public int IdTipoComision { get; set; }
        public decimal PorcComision { get; set; }
        public decimal TotalComision { get; set; }

        public int? IdPersonalCc { get; set; }

        public string? Personal { get; set; }
        public string? Cargo { get; set; }
        public string? TipoComision { get; set; }
    }

    public class VMVentaCobro
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }
        public int IdMoneda { get; set; }
        public int IdCuenta { get; set; } // obligatorio si hay cobro
        public decimal Importe { get; set; }

        public decimal Cotizacion { get; set; }  // si no usás, 1
        public decimal Conversion { get; set; }  // si no usás, Importe

        public string? NotaCliente { get; set; }
        public string? NotaInterna { get; set; }

        // CC/caja (dejar null)
        public int? IdClienteCc { get; set; }
        public int? IdArtistaCc { get; set; }
        public int? IdCaja { get; set; }

        // textos
        public string? Moneda { get; set; }
        public string? Cuenta { get; set; }
    }
}