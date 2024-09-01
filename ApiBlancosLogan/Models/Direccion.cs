using System;
using System.Collections.Generic;

namespace ApiBlancosLogan.Models;

public partial class Direccion
{
    public long IdDireccion { get; set; }

    public string CodigoPostal { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public string Municipio { get; set; } = null!;

    public string Colonia { get; set; } = null!;

    public string Calle { get; set; } = null!;

    public string Numero { get; set; } = null!;

    public string Telefono { get; set; } = null!;
}
