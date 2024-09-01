using System;
using System.Collections.Generic;

namespace ApiBlancosLogan.Models;

public partial class Termino
{
    public long IdTerminos { get; set; }

    public DateTime Fecha { get; set; }

    public long IdCliente { get; set; }

    public string ArchivoVersion { get; set; } = null!;
}
