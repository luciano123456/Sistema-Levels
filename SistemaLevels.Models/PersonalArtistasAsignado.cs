using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class PersonalArtistasAsignado
{
    public int Id { get; set; }

    public int IdPersonal { get; set; }

    public int IdArtista { get; set; }

    public virtual Artista IdArtistaNavigation { get; set; } = null!;

    public virtual Personal IdPersonalNavigation { get; set; } = null!;
}
