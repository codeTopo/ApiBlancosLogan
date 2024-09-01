

namespace ApiBlancosLogan.Request
{
    public class VentaRequestGet
    {
        public long? IdVenta { get; set; }
        public long Cliente { get; set; }
        public string? Pago {  get; set; }
        public List<ClienteV>? clientesV { get; set; }
        public List<DireccionV>? DireccionV { get; set; }
        public List<ConceptosV>? Pedido { get; set; }
        public decimal Total { get; set; }
    }

    public class ClienteV
    {
        public long? idCliente { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
    }
    public class DireccionV
    {
        public long IdDireccion { get; set; }
        public string Calle { get; set; } = null!;
        public string Colonia { get; set; } = null!;
        public string Municipio { get; set; } = null!;
        public string? Numero { get; set; }
        public string Estado { get; set; } = null!;
        public string? Cp { get; set; }
    }
    public class ConceptosV
    {

        public int idProducto { get; set; }
        public List<ProductoV>? ProductoV { get; set; }
        public int? Cantidad { get; set; }
        public decimal? Precio { get; set; }
    }
    public class ProductoV
    {
        public string? Nombre { get; set; }
        public string? Ubicacion { get; set; }
    }
}
