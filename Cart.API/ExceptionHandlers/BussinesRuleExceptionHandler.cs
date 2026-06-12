using Microsoft.AspNetCore.Diagnostics;
using Cart.API.Exceptions;

namespace Cart.API.ExceptionHandlers
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
                "CRT-003" => 422,
                "CRT-004" => 400,
                _ => 400
            };

            await context.Response.WriteAsJsonAsync(new
            {
                type = ex.ErrorCode switch
                {
                    "CRT-003" => "https://tools.ietf.org/html/rfc4918#section-11.2",
                    _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                },
                title = ex.ErrorCode switch
                {
                    "CRT-003" => "Unprocessable Entity",
                    _ => "Bad Request"
                },
                status = context.Response.StatusCode,
                detail = ex.ErrorCode switch
                {
                    "CRT-003" => "No se puede procesar la solicitud.",
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