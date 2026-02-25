using System;
using System.Collections.Generic;

namespace SistemaLevels.Models;

public partial class ClientesProductorasAsignada
{
    public int Id { get; set; }

    public int? IdProductora { get; set; }

    public int? IdCliente { get; set; }

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual Productora? IdNavigation { get; set; }
}
