using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _service;

        public NotificationsController(NotificationService service)
        {
            _service = service;
        }

        [HttpPost("send")]
        [ProducesResponseType(typeof(NotificationResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
        {
            var result = await _service.SendAsync(request);
            return StatusCode(201, result);
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var result = await _service.GetByUserIdAsync(userId);
            return Ok(result);
        }
    }
}