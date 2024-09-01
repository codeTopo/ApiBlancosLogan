using System.ComponentModel.DataAnnotations;

namespace ApiBlancosLogan.Request
{
    public class TerminosRequest
    {
        public long IdTerminos { get; set; }
        public DateTime Fecha { get; set; }
        [Required(ErrorMessage = "El campo idCliente es obligatorio.")]
        public long IdCliente { get; set; }
        public string ArchivoVersion { get; set; } = null!;
    }
}
