using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class ClientesProductora
{
    public int Id { get; set; }

    public int IdCliente { get; set; }

    public int IdProductora { get; set; }

    public byte OrigenAsignacion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Productora IdProductoraNavigation { get; set; } = null!;
}
