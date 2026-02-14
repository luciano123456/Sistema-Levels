using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class TiposComisione
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<VentasPersonal> VentasPersonals { get; set; } = new List<VentasPersonal>();
}
