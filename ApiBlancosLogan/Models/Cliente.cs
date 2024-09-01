using System;
using System.Collections.Generic;

namespace ApiBlancosLogan.Models;

public partial class Cliente
{
    public long IdCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Telefono { get; set; } = null!;
}
