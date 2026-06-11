using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;
using Notifications.API.Repository;

namespace Notifications.API.Services
{
    public class NotificationService
    {
        private readonly NotificationRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public NotificationService(NotificationRepository repository, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(Guid usuarioId)
        {
            var notifications = await _repository.GetByUserIdAsync(usuarioId);
            if (!notifications.Any())
                throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");
            return notifications.Select(MapToResponse);
        }

        public async Task<NotificationResponse> SendAsync(SendNotificationRequest request)
        {
            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(request.Mensaje) ||
                string.IsNullOrWhiteSpace(request.Tipo))
                throw new BusinessRuleException("NTF-002", "Los datos de la notificación son inválidos.");

            // Validar que el tipo sea válido
            var tiposValidos = new[] { "Email", "Push", "SMS" };
            if (!tiposValidos.Contains(request.Tipo))
                throw new BusinessRuleException("NTF-002", "Los datos de la notificación son inválidos.");

            // Validar que el usuario existe
            var client = _httpClientFactory.CreateClient("default");
            var usersUrl = _config["Services:UsersAPI"];
            var userResponse = await client.GetAsync($"{usersUrl}/api/users/{request.UsuarioId}");
            if (!userResponse.IsSuccessStatusCode)
                throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                Mensaje = request.Mensaje,
                Tipo = request.Tipo,
                Estado = "Enviada"
            };

            var created = await _repository.CreateAsync(notification);
            return MapToResponse(created);
        }

        private NotificationResponse MapToResponse(Notification notification) => new()
        {
            Id = notification.Id,
            UsuarioId = notification.UsuarioId,
            Mensaje = notification.Mensaje,
            Tipo = notification.Tipo,
            Estado = notification.Estado,
            FechaEnvio = notification.FechaEnvio
        };
    }
}