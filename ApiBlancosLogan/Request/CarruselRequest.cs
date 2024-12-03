using System.ComponentModel.DataAnnotations;

namespace ApiBlancosLogan.Request
{
    public class CarruselRequest
    {
        public int IdCarrusel { get; set; }
        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        public string? Nombre { get; set; }
        [Required(ErrorMessage = "El campo Imagen es obligatorio.")]
        public string? Imagen { get; set; }
    }

    public class CarruselUpdate
    {
        public string? Nombre { get; set; }
        public string? Imagen { get; set; }
    }
}
  