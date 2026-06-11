using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<BusinessRuleExceptionHandler> _logger;

        public BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex) return false;

            var correlationId = context.Items["X-Correlation-Id"]?.ToString();
            _logger.LogWarning("Error de negocio: {ErrorCode} - {Message} | CorrelationId: {CorrelationId}", ex.ErrorCode, ex.Message, correlationId);

            context.Response.StatusCode = 400;

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "La solicitud no es válida.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId
            }, cancellationToken);
            return true;
        }
    }
}