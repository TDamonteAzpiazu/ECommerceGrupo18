using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;
using Products.API.Repository;

namespace Products.API.Services
{
    public class ProductsService
    {
        private readonly ProductsRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public ProductsService(ProductsRepository repository, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<IEnumerable<ProductResponse>> GetAllAsync(string? categoria, string? nombre)
        {
            var products = await _repository.GetAllAsync(categoria, nombre);
            return products.Select(MapToResponse);
        }

        public async Task<ProductResponse> GetByIdAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");
            return MapToResponse(product);
        }

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Categoria) ||
                request.Precio <= 0 ||
                request.Stock < 0)
                throw new BusinessRuleException("PRD-002", "Los datos del producto son inválidos.");

            var exists = await _repository.ExistsWithNameAndCategoriaAsync(request.Nombre, request.Categoria);
            if (exists)
                throw new BusinessRuleException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Precio = request.Precio,
                Stock = request.Stock,
                Categoria = request.Categoria
            };

            var created = await _repository.CreateAsync(product);
            return MapToResponse(created);
        }

        public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Categoria) ||
                request.Precio <= 0 ||
                request.Stock < 0)
                throw new BusinessRuleException("PRD-002", "Los datos del producto son inválidos.");

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            var exists = await _repository.ExistsWithNameAndCategoriaAsync(request.Nombre, request.Categoria, id);
            if (exists)
                throw new BusinessRuleException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

            existing.Nombre = request.Nombre;
            existing.Descripcion = request.Descripcion;
            existing.Precio = request.Precio;
            existing.Stock = request.Stock;
            existing.Categoria = request.Categoria;

            var updated = await _repository.UpdateAsync(id, existing);
            return MapToResponse(updated!);
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            var client = _httpClientFactory.CreateClient();
            var ordersUrl = _config["Services:OrdersAPI"];
            var response = await client.GetAsync($"{ordersUrl}/api/orders/product/{id}/active");
            var hasActiveOrders = await response.Content.ReadFromJsonAsync<bool>();

            if (hasActiveOrders)
                throw new BusinessRuleException("PRD-004", "El producto tiene órdenes activas y no puede eliminarse.");

            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }

        private ProductResponse MapToResponse(Product product) => new()
        {
            Id = product.Id,
            Nombre = product.Nombre,
            Descripcion = product.Descripcion,
            Precio = product.Precio,
            Stock = product.Stock,
            Categoria = product.Categoria,
            FechaCreacion = product.FechaCreacion
        };

        public async Task UpdateStockAsync(Guid id, int nuevoStock)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");
            await _repository.UpdateStockAsync(id, nuevoStock);
        }
    }
}