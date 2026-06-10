namespace Cart.API.Models
{
    public class ShoppingCart
    {
        public Guid UsuarioId { get; set; }
        public List<ShoppingCartItem> Items { get; set; } = new();
        public DateTime FechaActualizacion { get; set; }
    }

    public class ShoppingCartItem
    {
        public Guid UsuarioId { get; set; }
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
}