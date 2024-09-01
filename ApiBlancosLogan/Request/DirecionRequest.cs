using System.ComponentModel.DataAnnotations;

namespace ApiBlancosLogan.Request
{
    public class DirecionRequest
    {
        public long IdDireccion { get; set; }

        [Required(ErrorMessage = "El campo Calle es obligatorio.")]
        public string Calle { get; set; } = null!;

        [Required(ErrorMessage = "El campo Colonia es obligatorio.")]
        public string Colonia { get; set; } = null!;
        [Required(ErrorMessage = "El campo Municipio es obligatorio.")]
        public string Municipio { get; set; } = null!;
        [Required(ErrorMessage = "El campo Numero es obligatorio.")]
        [MaxLength(10, ErrorMessage = "EL Numero solo puede tener hasta 10 digitos")]
        public string? Numero { get; set; }
        [Required(ErrorMessage = "El campo Estado es obligatorio.")]
        public string Estado { get; set; } = null!;
        [Required(ErrorMessage = "El campo Codigo Postal es obligatorio.")]
        [MaxLength(5, ErrorMessage ="EL Codigo Postal solo puede tener 5 digitos")]
        public string? Cp { get; set; }
        [Required(ErrorMessage = "El campo Telefono es obligatorio.")]
        [StringLength(10, ErrorMessage = "El campo Telefono debe tener 10 caracteres.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El campo Telefono debe contener solo numeros.")]
        public string? Telefono {  get; set; }
    }
}
