using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class OpcionesBinaria
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
