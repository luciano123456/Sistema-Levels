namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMPersonal
    {
        public int Id { get; set; }

        public string Nombre { get; set; }
        public string? Dni { get; set; }

        public int IdPais { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }

        public int? IdCondicionIva { get; set; }

        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        // textos
        public string Pais { get; set; }
        public string TipoDocumento { get; set; }
        public string CondicionIva { get; set; }

        // auditoría
        public int IdUsuarioRegistra { get; set; }
        public DateTime FechaRegistra { get; set; }
        public string UsuarioRegistra { get; set; }

        public int? IdUsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }
        public string UsuarioModifica { get; set; }
    }

}
