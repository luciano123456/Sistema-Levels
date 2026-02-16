namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMRepresentante
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;
        public string Dni { get; set; } = null!;

        public int IdPais { get; set; }
        public int? IdTipoDocumento { get; set; }

        public string? NumeroDocumento { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }

        // auditoría
        public int IdUsuarioRegistra { get; set; }
        public DateTime FechaRegistra { get; set; }

        public int? IdUsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }

        // datos de navegación para mostrar
        public string? UsuarioRegistra { get; set; }
        public string? UsuarioModifica { get; set; }
        public string? Pais { get; set; }
        public string? TipoDocumento { get; set; }
    }
}
