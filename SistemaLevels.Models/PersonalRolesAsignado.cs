using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class PersonalRolesAsignado
{
    public int Id { get; set; }

    public int IdPersonal { get; set; }

    public int IdRol { get; set; }

    public virtual Personal IdPersonalNavigation { get; set; } = null!;

    public virtual PersonalRol IdRolNavigation { get; set; } = null!;
}
