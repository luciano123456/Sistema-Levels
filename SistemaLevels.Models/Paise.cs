using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class Paise
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Artista> Artista { get; set; } = new List<Artista>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<PaisesCondicionesIva> PaisesCondicionesIvas { get; set; } = new List<PaisesCondicionesIva>();

    public virtual ICollection<PaisesMoneda> PaisesMoneda { get; set; } = new List<PaisesMoneda>();

    public virtual ICollection<PaisesProvincia> PaisesProvincia { get; set; } = new List<PaisesProvincia>();

    public virtual ICollection<PaisesTiposDocumento> PaisesTiposDocumentos { get; set; } = new List<PaisesTiposDocumento>();

    public virtual ICollection<Personal> Personals { get; set; } = new List<Personal>();

    public virtual ICollection<Productora> Productoras { get; set; } = new List<Productora>();

    public virtual ICollection<Representante> Representantes { get; set; } = new List<Representante>();
}
