using System;
using System.Collections.Generic;

namespace ApiBlancosLogan.Models;

public partial class Usuario
{
    public long IdUsuario { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}
