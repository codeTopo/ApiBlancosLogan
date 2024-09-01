using System;
using System.Collections.Generic;

namespace ApiBlancosLogan.Models;

public partial class Producto
{
    public long IdProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public decimal Precio { get; set; }

    public string Ubicacion { get; set; } = null!;
}
