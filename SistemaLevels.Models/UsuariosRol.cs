using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class UsuariosRol
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<User> Usuarios { get; set; } = new List<User>();
}
