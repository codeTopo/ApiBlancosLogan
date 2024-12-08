using System;
using System.Collections.Generic;

namespace ApiBlancosLogan.Models;

public partial class Pago
{
    public int IdPagos { get; set; }

    public string PaymentId { get; set; } = null!;

    public string? Estado { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string? TipoEvento { get; set; }

    public string? Accion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public string? ReferenciaInterna { get; set; }
}
