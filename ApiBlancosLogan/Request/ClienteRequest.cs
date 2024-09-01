using System.ComponentModel.DataAnnotations;

namespace ApiBlancosLogan.Request
{
    public class ClienteRequest
    {
        public long IdCliente { get; set; }
        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        public string? Nombre { get; set; }
        [Required(ErrorMessage = "El campo Apellido es obligatorio.")]
        public string? Apellidos { get; set; }
        [Required(ErrorMessage = "El campo Telefono es obligatorio.")]
        [StringLength(10, ErrorMessage = "El campo Telefono debe tener 10 caracteres.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El campo Telefono debe contener solo numeros.")]
        public string? Telefono { get; set; }
    }

    public class ClienteUpdate
    {
        public string? Nombre { get; set; }
        public string? Apellidos { get; set;}
        [StringLength(10, ErrorMessage = "El campo Telefono debe tener 10 caracteres.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El campo Telefono debe contener solo numeros.")]
        public string? Telefono { get; set; }

    }
}
