using Microsoft.AspNetCore.Diagnostics;

namespace Cart.API.ExceptionHandlers
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
            _logger.LogError(exception, "Error inesperado");

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrió un error inesperado.",
                instance = context.Request.Path.Value,
                errorCode = "CRT-005",
                errorMessage = "Error interno al procesar el carrito."
            }, cancellationToken);
            return true;
        }
    }
}