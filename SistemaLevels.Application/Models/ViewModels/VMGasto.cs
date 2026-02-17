namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMGasto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int IdCategoria { get; set; }
        public int IdMoneda { get; set; }
        public int IdCuenta { get; set; }
        public int? IdPersonal { get; set; }

        public string Concepto { get; set; } = null!;
        public decimal Importe { get; set; }
        public string? NotaInterna { get; set; }

        // Auditoría
        public int IdUsuarioRegistra { get; set; }
        public DateTime FechaRegistra { get; set; }

        public int? IdUsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }

        // Navegación
        public string? Categoria { get; set; }
        public string? Moneda { get; set; }
        public string? Cuenta { get; set; }
        public string? Personal { get; set; }

        public string? UsuarioRegistra { get; set; }
        public string? UsuarioModifica { get; set; }
    }
}
