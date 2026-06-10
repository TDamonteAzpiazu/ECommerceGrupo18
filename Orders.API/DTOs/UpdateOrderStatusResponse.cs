namespace Orders.API.DTOs
{
    public class UpdateOrderStatusResponse
    {
        public Guid Id { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaActualizacion { get; set; }
    }
}