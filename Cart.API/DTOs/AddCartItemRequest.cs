namespace Cart.API.DTOs
{
    public class AddCartItemRequest
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
}