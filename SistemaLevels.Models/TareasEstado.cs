using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class TareasEstado
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
}
