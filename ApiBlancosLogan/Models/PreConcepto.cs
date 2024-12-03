using System;
using System.Collections.Generic;

namespace ApiBlancosLogan.Models;

public partial class PreConcepto
{
    public long IdPreConcepto { get; set; }

    public Guid IdPrePago { get; set; }

    public long IdProducto { get; set; }

    public decimal Cantidad { get; set; }
}
