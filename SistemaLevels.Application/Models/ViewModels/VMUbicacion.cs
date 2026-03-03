using System;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMUbicacion
    {
        public int Id { get; set; }

        public string Descripcion { get; set; } = "";

        public string Espacio { get; set; } = "";

        public string Direccion { get; set; } = "";

        // Auditoría
        public int? IdUsuarioRegistra { get; set; }
        public DateTime? FechaRegistra { get; set; }
        public string? UsuarioRegistra { get; set; }

        public int? IdUsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }
        public string? UsuarioModifica { get; set; }
    }
}