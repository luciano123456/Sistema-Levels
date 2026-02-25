using System;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMCliente
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public string? Telefono { get; set; }
        public string? TelefonoAlternativo { get; set; }

        public string Dni { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }

        public string? Email { get; set; }

        public int IdPais { get; set; }
        public int IdProvincia { get; set; }

        public string? Localidad { get; set; }
        public string? EntreCalles { get; set; }
        public string? Direccion { get; set; }
        public string? CodigoPostal { get; set; }

        public int? IdCondicionIva { get; set; }

        // datos descriptivos
        public string? Productora { get; set; }
        public string? Pais { get; set; }
        public string? Provincia { get; set; }
        public string? TipoDocumento { get; set; }
        public string? CondicionIva { get; set; }

        public int? AsociacionAutomatica { get; set; }

        // 🔥 NUEVO
        public List<int> ProductorasIds { get; set; } = new();

        // auditoría
        public int IdUsuarioRegistra { get; set; }
        public DateTime FechaRegistra { get; set; }
        public string? UsuarioRegistra { get; set; }

        public int? IdUsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }
        public string? UsuarioModifica { get; set; }
    }
}
