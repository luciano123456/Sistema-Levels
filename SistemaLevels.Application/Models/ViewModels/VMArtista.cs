namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMArtista
    {
        public int Id { get; set; }

        public string Nombre { get; set; }
        public string NombreArtistico { get; set; }

        public string Telefono { get; set; }
        public string TelefonoAlternativo { get; set; }

        public string Dni { get; set; }

        public int IdPais { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }

        public string Email { get; set; }

        public int IdProductora { get; set; }
        public int IdProvincia { get; set; }

        public string Localidad { get; set; }
        public string EntreCalles { get; set; }
        public string Direccion { get; set; }
        public string CodigoPostal { get; set; }

        public int? IdCondicionIva { get; set; }

        public int IdRepresentante { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public int IdMoneda { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioNegMax { get; set; }
        public decimal PrecioNegMin { get; set; }

        // Datos de navegación
        public string Pais { get; set; }
        public string TipoDocumento { get; set; }
        public string CondicionIva { get; set; }
        public string Provincia { get; set; }
        public string Representante { get; set; }
        public string Productora { get; set; }
        public string Moneda { get; set; }

        // Auditoría
        public int IdUsuarioRegistra { get; set; }
        public DateTime FechaRegistra { get; set; }
        public string UsuarioRegistra { get; set; }

        public int? IdUsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }
        public string UsuarioModifica { get; set; }


    }
}
