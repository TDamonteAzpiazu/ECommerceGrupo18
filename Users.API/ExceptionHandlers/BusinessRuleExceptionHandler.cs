using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
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

            context.Response.StatusCode = ex.ErrorCode switch
            {
                "USR-003" => 401,
                "USR-004" => 403,
                "USR-005" => 403,
                "USR-001" => 409,
                _ => 400
            };

            await context.Response.WriteAsJsonAsync(new
            {
                type = ex.ErrorCode switch
                {
                    "USR-003" => "https://tools.ietf.org/html/rfc7235#section-3.1",
                    "USR-004" => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    "USR-005" => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    "USR-001" => "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                },
                title = ex.ErrorCode switch
                {
                    "USR-003" => "Unauthorized",
                    "USR-004" => "Forbidden",
                    "USR-005" => "Forbidden",
                    "USR-001" => "Conflict",
                    _ => "Bad Request"
                },
                status = context.Response.StatusCode,
                detail = ex.ErrorCode switch
                {
                    "USR-003" => "Las credenciales no son válidas.",
                    "USR-004" => "El acceso está prohibido.",
                    "USR-005" => "El acceso está prohibido.",
                    "USR-001" => "Ya existe un recurso con esos datos.",
                    _ => "La solicitud no es válida."
                },
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId
            }, cancellationToken);
            return true;
        }
    }
}