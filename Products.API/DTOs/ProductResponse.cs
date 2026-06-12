namespace Products.API.DTOs
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}
// Hay solo un response, pq en la consigna muestra siempre lo mismo como respuesta