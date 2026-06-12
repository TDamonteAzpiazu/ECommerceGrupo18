using Microsoft.AspNetCore.Diagnostics;

namespace Orders.API.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            var correlationId = context.Items["X-Correlation-Id"]?.ToString();
            _logger.LogError(exception, "Error inesperado: {CorrelationId}", correlationId);

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrió un error inesperado.",
                instance = context.Request.Path.Value,
                errorCode = "ORD-007",
                errorMessage = "Error interno al procesar la orden.",
                correlationId
            }, cancellationToken);
            return true;
        }
    }
}