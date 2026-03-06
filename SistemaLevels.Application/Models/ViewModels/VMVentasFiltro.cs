using SistemaLevels.Models;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMVentasFiltro
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }

        public int? IdEstado { get; set; }
        public int? IdArtista { get; set; }

        public int? IdCliente { get; set; }
    }
}
