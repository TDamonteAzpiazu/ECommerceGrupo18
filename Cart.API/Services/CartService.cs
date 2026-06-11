using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Models;
using Cart.API.Repository;
using System.Text.Json;

namespace Cart.API.Services
{
    public class CartService
    {
        private readonly CartRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public CartService(CartRepository repository, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<CartResponse> GetByUserIdAsync(Guid usuarioId)
        {
            var cart = await _repository.GetByUserIdAsync(usuarioId);
            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");
            return MapToResponse(cart);
        }

        public async Task<CartResponse> AddItemAsync(Guid usuarioId, AddCartItemRequest request)
        {
            if (request.Cantidad <= 0)
                throw new BusinessRuleException("CRT-004", "Cantidad inválida.");

            // Verificar que el producto existe y tiene stock
            var client = _httpClientFactory.CreateClient("default");
            var productsUrl = _config["Services:ProductsAPI"];
            var productResponse = await client.GetAsync($"{productsUrl}/api/products/{request.ProductoId}");

            if (!productResponse.IsSuccessStatusCode)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            var productJson = await productResponse.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<JsonElement>(productJson);
            var stock = product.GetProperty("stock").GetInt32();

            if (request.Cantidad > stock)
                throw new BusinessRuleException("CRT-003",
                    $"Stock insuficiente. Disponible: {stock}, solicitado: {request.Cantidad}.");

            await _repository.UpsertItemAsync(usuarioId, request.ProductoId, request.Cantidad);

            var cart = await _repository.GetByUserIdAsync(usuarioId);
            return MapToResponse(cart!);
        }

        public async Task<CartResponse> UpdateItemAsync(Guid usuarioId, Guid productoId, UpdateCartItemRequest request)
        {
            if (request.Cantidad <= 0)
                throw new BusinessRuleException("CRT-004", "Cantidad inválida.");

            var cart = await _repository.GetByUserIdAsync(usuarioId);
            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            var item = await _repository.GetItemAsync(usuarioId, productoId);
            if (item == null)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            // Verificar stock
            var client = _httpClientFactory.CreateClient("default");
            var productsUrl = _config["Services:ProductsAPI"];
            var productResponse = await client.GetAsync($"{productsUrl}/api/products/{productoId}");

            if (!productResponse.IsSuccessStatusCode)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            var productJson = await productResponse.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<JsonElement>(productJson);
            var stock = product.GetProperty("stock").GetInt32();

            if (request.Cantidad > stock)
                throw new BusinessRuleException("CRT-003",
                    $"Stock insuficiente. Disponible: {stock}, solicitado: {request.Cantidad}.");

            await _repository.UpsertItemAsync(usuarioId, productoId, request.Cantidad);

            var updated = await _repository.GetByUserIdAsync(usuarioId);
            return MapToResponse(updated!);
        }

        public async Task RemoveItemAsync(Guid usuarioId, Guid productoId)
        {
            var cart = await _repository.GetByUserIdAsync(usuarioId);
            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            var removed = await _repository.RemoveItemAsync(usuarioId, productoId);
            if (!removed)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");
        }

        public async Task ClearAsync(Guid usuarioId)
        {
            var cart = await _repository.GetByUserIdAsync(usuarioId);
            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            await _repository.ClearAsync(usuarioId);
        }

        private CartResponse MapToResponse(ShoppingCart cart) => new()
        {
            UsuarioId = cart.UsuarioId,
            Items = cart.Items.Select(i => new CartItemResponse
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad
            }).ToList(),
            FechaActualizacion = cart.FechaActualizacion
        };
    }
}