namespace Cart.API.DTOs
{
    public class CartResponse
    {
        public Guid UsuarioId { get; set; }
        public List<CartItemResponse> Items { get; set; } = new();
        public DateTime FechaActualizacion { get; set; }
    }

    public class CartItemResponse
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
}