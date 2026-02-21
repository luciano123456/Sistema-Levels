using SistemaLevels.Models;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMProductora
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public string? Telefono { get; set; }
        public string? TelefonoAlternativo { get; set; }

        public string? Dni { get; set; }

        public int? Idpais { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }

        public int? IdCondicionIva { get; set; }

        public string? Email { get; set; }

        public int IdProvincia { get; set; }
        public string? Localidad { get; set; }
        public string? EntreCalles { get; set; }
        public string? Direccion { get; set; }
        public string? CodigoPostal { get; set; }

        // auditoría
        public int IdUsuarioRegistra { get; set; }
        public DateTime FechaRegistra { get; set; }

        public int? IdUsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }

        // navegación
        public string? UsuarioRegistra { get; set; }
        public string? UsuarioModifica { get; set; }

        public string? Pais { get; set; }
        public string? TipoDocumento { get; set; }
        public string? CondicionIva { get; set; }
        public string? Provincia { get; set; }

        // ✅ NUEVO: clientes asignados
        public List<int> ClientesIds { get; set; } = new();
    }

    public class ProductorasClientesAsignados
    {
        public int Id { get; set; }
        public int IdProductora { get; set; }
        public int IdCliente { get; set; }

        public virtual Productora IdProductoraNavigation { get; set; } = null!;
        public virtual Cliente IdClienteNavigation { get; set; } = null!;
    }
}
