using System.ComponentModel.DataAnnotations;

namespace ApiBlancosLogan.Request
{
    public class VentaRequesPost
    {
        [Required(ErrorMessage = "El campo idCliente es obligatorio.")]
        public long? IdCliente { get; set; }
        [Required(ErrorMessage = "El campo Direccion es obligatorio.")]
        public long? IdDireccion { get; set; }
        public string? Pago { get; set; }
        [Required(ErrorMessage = "El campo Pedido es obligatorio.")]
        public List<ConceptoPost>? Pedido { get; set; }
    }

    public class ConceptoPost
    {
        [Required(ErrorMessage = "El campo idProducto es obligatorio.")]
        public long? IdProducto { get; set; }
        [Required(ErrorMessage = "El campo Cantidad es obligatorio.")]
        public decimal? Cantidad { get; set; }
      
    }
}
