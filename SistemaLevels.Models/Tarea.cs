using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class Tarea
{
    public int Id { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateTime FechaRegistra { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public DateTime? FechaModifica { get; set; }

    public DateTime Fecha { get; set; }

    public DateTime FechaLimite { get; set; }

    public int IdPersonal { get; set; }

    public string Descripcion { get; set; } = null!;

    public int IdEstado { get; set; }

    public virtual TareasEstado IdEstadoNavigation { get; set; } = null!;

    public virtual Personal IdPersonalNavigation { get; set; } = null!;

    public virtual User? IdUsuarioModificaNavigation { get; set; }

    public virtual User IdUsuarioRegistraNavigation { get; set; } = null!;
}
