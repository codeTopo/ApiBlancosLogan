using System;
using System.Collections.Generic;

namespace ApiBlancosLogan.Models;

public partial class PrePago
{
    public Guid IdPrePago { get; set; }

    public long IdCliente { get; set; }

    public long IdDireccion { get; set; }

    public bool EsFinalizado { get; set; }
}
