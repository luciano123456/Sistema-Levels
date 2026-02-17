using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class PersonalRol
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<PersonalRolesAsignado> PersonalRolesAsignados { get; set; } = new List<PersonalRolesAsignado>();
}
