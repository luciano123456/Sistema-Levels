using Microsoft.AspNetCore.Mvc;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMMonedaActualizacion
    {
        public int Id { get; set; }
        public decimal Cotizacion { get; set; }
    }
}
