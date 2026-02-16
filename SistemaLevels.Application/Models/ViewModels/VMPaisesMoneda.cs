using System;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMPaisesMoneda
    {
        public int Id { get; set; }

        public int IdPais { get; set; }
        public string? Pais { get; set; }

        public string Nombre { get; set; } = "";
        public decimal Cotizacion { get; set; }
    }
}
