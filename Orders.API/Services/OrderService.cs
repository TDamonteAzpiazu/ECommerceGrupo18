using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;
using Orders.API.Repository;
using System.Text.Json;

namespace Orders.API.Services
{
    public class OrderService
    {
        private readonly OrderRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public OrderService(OrderRepository repository, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<IEnumerable<OrderResponse>> GetAllAsync(Guid? usuarioId)
        {
            var orders = await _repository.GetAllAsync(usuarioId);
            return orders.Select(MapToResponse);
        }

        public async Task<OrderResponse> GetByIdAsync(Guid id)
        {
            var order = await _repository.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException("ORD-001", "Orden no encontrada.");
            return MapToResponse(order);
        }

        public async Task<OrderResponse> CreateAsync(CreateOrderRequest request)
        {
            // Validar campos
            if (request.Items == null || request.Items.Count == 0)
                throw new BusinessRuleException("ORD-002", "Los datos de la orden son inválidos.");

            var client = _httpClientFactory.CreateClient();

            // Validar que el usuario existe
            var usersUrl = _config["Services:UsersAPI"];
            var userResponse = await client.GetAsync($"{usersUrl}/api/users/{request.UsuarioId}");
            if (!userResponse.IsSuccessStatusCode)
                throw new NotFoundException("ORD-003", "Usuario no encontrado al crear la orden.");

            // Validar productos y stock
            var productsUrl = _config["Services:ProductsAPI"];
            var orderItems = new List<OrderItem>();
            decimal total = 0;

            foreach (var item in request.Items)
            {
                var productResponse = await client.GetAsync($"{productsUrl}/api/products/{item.ProductoId}");
                if (!productResponse.IsSuccessStatusCode)
                    throw new NotFoundException("ORD-004", "Producto no encontrado al crear la orden.");

                var productJson = await productResponse.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<JsonElement>(productJson);

                var stock = product.GetProperty("stock").GetInt32();
                var precio = product.GetProperty("precio").GetDecimal();
                var nombre = product.GetProperty("nombre").GetString();

                if (item.Cantidad > stock)
                    throw new BusinessRuleException("ORD-005",
                        $"Stock insuficiente para '{nombre}'. Disponible: {stock}, solicitado: {item.Cantidad}.");

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precio
                });

                total += precio * item.Cantidad;
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                Items = orderItems,
                Total = total,
                Estado = "Pendiente"
            };

            var created = await _repository.CreateAsync(order);
            return MapToResponse(created);
        }

        public async Task<UpdateOrderStatusResponse> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Estado))
                throw new BusinessRuleException("ORD-002", "Los datos de la orden son inválidos.");

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new NotFoundException("ORD-001", "Orden no encontrada.");

            // Validar transición de estado
            var validTransitions = new Dictionary<string, List<string>>
            {
                { "Pendiente",  new List<string> { "Confirmada", "Cancelada" } },
                { "Confirmada", new List<string> { "Enviada", "Cancelada" } },
                { "Enviada",    new List<string> { "Entregada" } },
                { "Entregada",  new List<string>() },
                { "Cancelada",  new List<string>() }
            };

            if (!validTransitions.ContainsKey(existing.Estado) ||
                !validTransitions[existing.Estado].Contains(request.Estado))
                throw new BusinessRuleException("ORD-006",
                    $"Una orden en estado '{existing.Estado}' no puede volver a '{request.Estado}'.");

            var updated = await _repository.UpdateStatusAsync(id, request.Estado);

            return new UpdateOrderStatusResponse
            {
                Id = updated!.Id,
                Estado = updated.Estado,
                FechaActualizacion = DateTime.UtcNow
            };
        }

        private OrderResponse MapToResponse(Order order) => new()
        {
            Id = order.Id,
            UsuarioId = order.UsuarioId,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList(),
            Total = order.Total,
            Estado = order.Estado,
            FechaCreacion = order.FechaCreacion
        };
    }
}