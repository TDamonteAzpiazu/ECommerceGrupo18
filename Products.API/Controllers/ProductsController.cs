using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs;
using Products.API.Services;

namespace Products.API.Controllers
{
    /// <summary>
    /// API para gestión de productos del eCommerce
    /// </summary>
    [ApiController]
    [Route("api/products")]
    [Tags("Products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsService _service;

        public ProductsController(ProductsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Listar todos los productos con filtros opcionales
        /// </summary>
        /// <param name="categoria">Filtrar por categoría (opcional)</param>
        /// <param name="nombre">Filtrar por nombre, búsqueda parcial (opcional)</param>
        /// <returns>Lista de productos</returns>
        /// <response code="200">Lista de productos obtenida correctamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductResponse>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetAll([FromQuery] string? categoria, [FromQuery] string? nombre)
        {
            var result = await _service.GetAllAsync(categoria, nombre);
            return Ok(result);
        }

        /// <summary>
        /// Obtener un producto por su ID
        /// </summary>
        /// <param name="id">ID del producto (GUID)</param>
        /// <returns>Producto encontrado</returns>
        /// <response code="200">Producto encontrado correctamente</response>
        /// <response code="404">PRD-001: Producto no encontrado</response>
        /// <response code="500">PRD-005: Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Crear un nuevo producto
        /// </summary>
        /// <param name="request">Datos del producto a crear</param>
        /// <returns>Producto creado</returns>
        /// <response code="201">Producto creado correctamente</response>
        /// <response code="400">PRD-002: Los datos del producto son inválidos</response>
        /// <response code="409">PRD-003: Ya existe un producto con ese nombre en la categoría</response>
        /// <response code="500">PRD-005: Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 409)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            var result = await _service.CreateAsync(request);
            return StatusCode(201, result);
        }

        /// <summary>
        /// Actualizar un producto existente
        /// </summary>
        /// <param name="id">ID del producto a actualizar (GUID)</param>
        /// <param name="request">Datos actualizados del producto</param>
        /// <returns>Producto actualizado</returns>
        /// <response code="200">Producto actualizado correctamente</response>
        /// <response code="400">PRD-002: Los datos del producto son inválidos</response>
        /// <response code="404">PRD-001: Producto no encontrado</response>
        /// <response code="500">PRD-005: Error interno del servidor</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return Ok(result);
        }

        /// <summary>
        /// Eliminar un producto
        /// </summary>
        /// <param name="id">ID del producto a eliminar (GUID)</param>
        /// <response code="204">Producto eliminado correctamente</response>
        /// <response code="404">PRD-001: Producto no encontrado</response>
        /// <response code="409">PRD-004: El producto tiene órdenes activas y no puede eliminarse</response>
        /// <response code="500">PRD-005: Error interno del servidor</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 409)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Actualizar el stock de un producto
        /// </summary>
        /// <remarks>Endpoint de uso interno. Lo llama Orders.API al confirmar o cancelar una orden.</remarks>
        /// <param name="id">ID del producto</param>
        /// <param name="request">Nuevo valor de stock</param>
        /// <response code="200">Stock actualizado correctamente</response>
        /// <response code="404">PRD-001: Producto no encontrado</response>
        [HttpPatch("{id}/stock")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
        {
            await _service.UpdateStockAsync(id, request.NuevoStock);
            return Ok();
        }
    }
}