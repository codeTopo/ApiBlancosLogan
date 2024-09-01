using System;
using System.Collections.Generic;

namespace ApiBlancosLogan.Models;

public partial class Ventum
{
    public long IdVenta { get; set; }

    public long IdCliente { get; set; }

    public long IdDirecion { get; set; }

    public long Total { get; set; }

    public DateTime Fecha { get; set; }

    public string Pago { get; set; } = null!;
}
