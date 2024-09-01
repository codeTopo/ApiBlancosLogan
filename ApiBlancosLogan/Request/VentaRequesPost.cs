using System.ComponentModel.DataAnnotations;

namespace ApiBlancosLogan.Request
{
    public class VentaRequesPost
    {
        public long IdVenta { get; set; }
        [Required(ErrorMessage = "El campo idCliente es obligatorio.")]
        public long? IdCliente { get; set; }
        [Required(ErrorMessage = "El campo Direccion es obligatorio.")]
        public long? IdDireccion { get; set; }
        [Required(ErrorMessage = "El campo Pedido es obligatorio.")]
        public string? Pago { get; set; }
        public List<ConceptoPost>? Pedido { get; set; }
    }

    public class ConceptoPost
    {
        public long IdConcepto { get; set; }
        [Required(ErrorMessage = "El campo idProducto es obligatorio.")]
        public long? IdProducto { get; set; }
        [Required(ErrorMessage = "El campo Cantidad es obligatorio.")]
        public decimal? Cantidad { get; set; }
      
    }
}
