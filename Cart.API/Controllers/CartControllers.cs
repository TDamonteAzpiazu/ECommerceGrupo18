using Microsoft.AspNetCore.Mvc;
using Cart.API.DTOs;
using Cart.API.Services;

namespace Cart.API.Controllers
{
    /// <summary>
    /// API para gestión del carrito de compras del eCommerce
    /// </summary>
    [ApiController]
    [Route("api/cart")]
    [Tags("Cart")]
    public class CartController : ControllerBase
    {
        private readonly CartService _service;

        public CartController(CartService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtener el carrito de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario (GUID)</param>
        /// <returns>Carrito del usuario</returns>
        /// <response code="200">Carrito obtenido correctamente</response>
        /// <response code="404">CRT-001: Carrito no encontrado</response>
        /// <response code="500">CRT-005: Error interno del servidor</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetCart(Guid userId)
        {
            var result = await _service.GetByUserIdAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Agregar un producto al carrito
        /// </summary>
        /// <param name="userId">ID del usuario (GUID)</param>
        /// <param name="request">Producto y cantidad a agregar</param>
        /// <returns>Carrito actualizado</returns>
        /// <response code="200">Producto agregado correctamente</response>
        /// <response code="400">CRT-004: Cantidad inválida</response>
        /// <response code="404">CRT-002: Producto no encontrado</response>
        /// <response code="422">CRT-003: Stock insuficiente</response>
        /// <response code="500">CRT-005: Error interno del servidor</response>
        [HttpPost("{userId}/items")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 422)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> AddItem(Guid userId, [FromBody] AddCartItemRequest request)
        {
            var result = await _service.AddItemAsync(userId, request);
            return Ok(result);
        }

        /// <summary>
        /// Actualizar la cantidad de un producto en el carrito
        /// </summary>
        /// <param name="userId">ID del usuario (GUID)</param>
        /// <param name="productId">ID del producto (GUID)</param>
        /// <param name="request">Nueva cantidad</param>
        /// <returns>Carrito actualizado</returns>
        /// <response code="200">Cantidad actualizada correctamente</response>
        /// <response code="400">CRT-004: Cantidad inválida</response>
        /// <response code="404">CRT-001: Carrito no encontrado / CRT-002: Producto no encontrado</response>
        /// <response code="422">CRT-003: Stock insuficiente</response>
        /// <response code="500">CRT-005: Error interno del servidor</response>
        [HttpPut("{userId}/items/{productId}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 422)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> UpdateItem(Guid userId, Guid productId, [FromBody] UpdateCartItemRequest request)
        {
            var result = await _service.UpdateItemAsync(userId, productId, request);
            return Ok(result);
        }

        /// <summary>
        /// Quitar un producto del carrito
        /// </summary>
        /// <param name="userId">ID del usuario (GUID)</param>
        /// <param name="productId">ID del producto (GUID)</param>
        /// <response code="204">Producto eliminado del carrito correctamente</response>
        /// <response code="404">CRT-001: Carrito no encontrado / CRT-002: Producto no encontrado</response>
        /// <response code="500">CRT-005: Error interno del servidor</response>
        [HttpDelete("{userId}/items/{productId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> RemoveItem(Guid userId, Guid productId)
        {
            await _service.RemoveItemAsync(userId, productId);
            return NoContent();
        }

        /// <summary>
        /// Vaciar el carrito completo de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario (GUID)</param>
        /// <response code="204">Carrito vaciado correctamente</response>
        /// <response code="404">CRT-001: Carrito no encontrado</response>
        /// <response code="500">CRT-005: Error interno del servidor</response>
        [HttpDelete("{userId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            await _service.ClearAsync(userId);
            return NoContent();
        }
    }
}