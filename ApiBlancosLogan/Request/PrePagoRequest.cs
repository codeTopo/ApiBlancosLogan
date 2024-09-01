using System.ComponentModel.DataAnnotations;

namespace ApiBlancosLogan.Request
{
    public class PrepagoRequest
    {
        public long IdVenta { get; set; }

        [Required(ErrorMessage = "El campo idCliente es obligatorio.")]
        public long? IdCliente { get; set; }

        [Required(ErrorMessage = "El campo Direccion es obligatorio.")]
        public long? IdDireccion { get; set; }

        [Required(ErrorMessage = "El campo Pedido es obligatorio.")]
        public string? Pago { get; set; }

        public List<PreConcepto>? Pedido { get; set; }
    }

    public class PreConcepto
    {
        public long IdConcepto { get; set; }

        [Required(ErrorMessage = "El campo idProducto es obligatorio.")]
        public long? IdProducto { get; set; }

        [Required(ErrorMessage = "El campo Cantidad es obligatorio.")]
        public decimal? Cantidad { get; set; }
    }

}
