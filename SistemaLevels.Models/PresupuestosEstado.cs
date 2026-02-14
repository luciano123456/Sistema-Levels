using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class PresupuestosEstado
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
}
