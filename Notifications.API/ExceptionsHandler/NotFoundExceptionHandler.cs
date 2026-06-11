using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
{
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<NotFoundExceptionHandler> _logger;

        public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not NotFoundException ex) return false;

            var correlationId = context.Items["X-Correlation-Id"]?.ToString();
            _logger.LogWarning("Recurso no encontrado: {ErrorCode} - {Message} | CorrelationId: {CorrelationId}", ex.ErrorCode, ex.Message, correlationId);

            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "El recurso solicitado no fue encontrado.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId
            }, cancellationToken);
            return true;
        }
    }
}