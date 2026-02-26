using SistemaLevels.Models;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMPersonalFiltro
    {
        public string? Nombre { get; set; }

        public int? IdPais { get; set; }
        public int? IdTipoDocumento { get; set; }
        public int? IdCondicionIva { get; set; }

        // ⭐ NUEVOS
        public int? IdRol { get; set; }
        public int? IdArtista { get; set; }
    }
}
