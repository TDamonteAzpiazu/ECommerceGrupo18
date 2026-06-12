using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers
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
                "PRD-003" => 409,
                "PRD-004" => 409,
                _ => 400
            };

            await context.Response.WriteAsJsonAsync(new
            {
                type = ex.ErrorCode switch
                {
                    "PRD-003" => "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    "PRD-004" => "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                },
                title = ex.ErrorCode switch
                {
                    "PRD-003" => "Conflict",
                    "PRD-004" => "Conflict",
                    _ => "Bad Request"
                },
                status = context.Response.StatusCode,
                detail = ex.ErrorCode switch
                {
                    "PRD-003" => "Ya existe un recurso con esos datos.",
                    "PRD-004" => "No se puede eliminar el recurso.",
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