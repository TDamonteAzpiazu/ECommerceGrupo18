using Microsoft.AspNetCore.Mvc;
using Cart.API.DTOs;
using Cart.API.Services;

namespace Cart.API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly CartService _service;

        public CartController(CartService service)
        {
            _service = service;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCart(Guid userId)
        {
            var result = await _service.GetByUserIdAsync(userId);
            return Ok(result);
        }

        [HttpPost("{userId}/items")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AddItem(Guid userId, [FromBody] AddCartItemRequest request)
        {
            var result = await _service.AddItemAsync(userId, request);
            return Ok(result);
        }

        [HttpPut("{userId}/items/{productId}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateItem(Guid userId, Guid productId, [FromBody] UpdateCartItemRequest request)
        {
            var result = await _service.UpdateItemAsync(userId, productId, request);
            return Ok(result);
        }

        [HttpDelete("{userId}/items/{productId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RemoveItem(Guid userId, Guid productId)
        {
            await _service.RemoveItemAsync(userId, productId);
            return NoContent();
        }

        [HttpDelete("{userId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            await _service.ClearAsync(userId);
            return NoContent();
        }
    }
}