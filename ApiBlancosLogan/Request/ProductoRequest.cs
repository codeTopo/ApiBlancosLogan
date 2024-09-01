using System.ComponentModel.DataAnnotations;

namespace ApiBlancosLogan.Request
{
    public class ProductoRequest
    {
        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        public string? Nombre { get; set; }
        [Required(ErrorMessage = "El campo Descripcion es obligatorio.")]
        public string? Descripcion { get; set; }
        [Required(ErrorMessage = "El campo Precio es obligatorio.")]
        public decimal? Precio { get; set; }
        [Required(ErrorMessage = "El campo Ubicacion es obligatorio.")]
        public string? Ubicacion { get; set; }
    }

    public class ProductoUpdate
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
        public string? Ubicacion { get; set; }
    }
}
