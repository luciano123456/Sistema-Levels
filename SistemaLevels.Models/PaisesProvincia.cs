using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class PaisesProvincia
{
    public int Id { get; set; }

    public int IdPais { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Artista> Artista { get; set; } = new List<Artista>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual Pais IdPaisNavigation { get; set; } = null!;

    public virtual ICollection<Productora> Productoras { get; set; } = new List<Productora>();
}
