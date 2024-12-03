
namespace ApiBlancosLogan.Request
{
    public class PrePagoRequestGet
    {
        public long? IdCliente { get; set; }
        public long? IdDireccion { get; set; }
        public string? Email { get; set; }
        public List<PreConceptosG>? Pedido { get; set; }
    }

    public class PreConceptosG
    {
        public long? IdPreConcepto { get; set; }
        public List<PrProducto>? ProductoV { get; set; }
        public long? IdProducto { get; set; }
        public decimal? Cantidad { get; set; }

    }

    public class PrProducto
    {
        public string? Nombre { get; set; }
        public string? Ubicacion { get; set; }
    }
}
