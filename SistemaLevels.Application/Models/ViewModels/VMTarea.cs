using System;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMTarea
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }
        public DateTime FechaLimite { get; set; }

        public int IdPersonal { get; set; }
        public string Personal { get; set; }

        public string Descripcion { get; set; }

        public int IdEstado { get; set; }
        public string Estado { get; set; }

        // Auditoría
        public int IdUsuarioRegistra { get; set; }
        public DateTime FechaRegistra { get; set; }
        public string UsuarioRegistra { get; set; }

        public int? IdUsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }
        public string UsuarioModifica { get; set; }
    }
}
