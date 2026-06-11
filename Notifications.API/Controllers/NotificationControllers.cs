using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers
{
    /// <summary>
    /// API para gestión de notificaciones del eCommerce
    /// </summary>
    [ApiController]
    [Route("api/notifications")]
    [Tags("Notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _service;

        public NotificationsController(NotificationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Registrar y simular el envío de una notificación
        /// </summary>
        /// <param name="request">Datos de la notificación a enviar</param>
        /// <returns>Notificación registrada</returns>
        /// <response code="201">Notificación enviada correctamente</response>
        /// <response code="400">NTF-002: Los datos de la notificación son inválidos</response>
        /// <response code="404">NTF-001: Usuario no encontrado</response>
        /// <response code="500">NTF-004: Error interno del servidor</response>
        [HttpPost("send")]
        [ProducesResponseType(typeof(NotificationResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
        {
            var result = await _service.SendAsync(request);
            return StatusCode(201, result);
        }

        /// <summary>
        /// Listar todas las notificaciones de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario (GUID)</param>
        /// <returns>Lista de notificaciones del usuario</returns>
        /// <response code="200">Notificaciones obtenidas correctamente</response>
        /// <response code="404">NTF-003: No se encontraron notificaciones para el usuario</response>
        /// <response code="500">NTF-004: Error interno del servidor</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var result = await _service.GetByUserIdAsync(userId);
            return Ok(result);
        }
    }
}