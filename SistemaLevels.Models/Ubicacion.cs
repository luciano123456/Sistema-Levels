using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class Ubicacion
{
    public int Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public string Espacio { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public virtual ICollection<PresupuestosDetalle> PresupuestosDetalles { get; set; } = new List<PresupuestosDetalle>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
