using System;
using System.Collections.Generic;

namespace ApiBlancosLogan.Models;

public partial class Concepto
{
    public long IdConcepto { get; set; }

    public long IdVenta { get; set; }

    public long IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal Precio { get; set; }
}
