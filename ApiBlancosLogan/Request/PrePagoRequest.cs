using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApiBlancosLogan.Request
{
    public class PrepagoRequest
    {

        [Required(ErrorMessage = "El campo idCliente es obligatorio.")]
        public long? IdCliente { get; set; }
        [Required(ErrorMessage = "El campo idCliente es obligatorio.")]
        public long? IdDireccion { get; set; }

        [Required(ErrorMessage = "El campo Correo Electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El campo Pedido es obligatorio.")]
        public List<PreConceptos>? Pedido { get; set; }
    }

    public class PreConceptos
    {

        [Required(ErrorMessage = "El campo idProducto es obligatorio.")]
        public long? IdProducto { get; set; }

        [Required(ErrorMessage = "El campo Cantidad es obligatorio.")]
        public decimal? Cantidad { get; set; }
    }
}
