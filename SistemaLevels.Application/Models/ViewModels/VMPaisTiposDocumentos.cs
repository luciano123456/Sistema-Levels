using SistemaLevels.Models;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMPaisesTiposDocumentos

    {
        public int Id { get; set; }
        public int IdPais { get; set; }

        public string Nombre { get; set; } = null!;
    }
}
