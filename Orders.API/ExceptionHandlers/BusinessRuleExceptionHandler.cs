using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex) return false;

            context.Response.StatusCode = ex.ErrorCode switch
            {
                "ORD-005" => 422,
                "ORD-006" => 409,
                _ => 400
            };

            await context.Response.WriteAsJsonAsync(new
            {
                type = ex.ErrorCode switch
                {
                    "ORD-005" => "https://tools.ietf.org/html/rfc4918#section-11.2",
                    "ORD-006" => "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                },
                title = ex.ErrorCode switch
                {
                    "ORD-005" => "Unprocessable Entity",
                    "ORD-006" => "Conflict",
                    _ => "Bad Request"
                },
                status = context.Response.StatusCode,
                detail = ex.ErrorCode switch
                {
                    "ORD-005" => "No se puede procesar la solicitud.",
                    "ORD-006" => "No se puede modificar el estado.",
                    _ => "La solicitud no es válida."
                },
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message
            }, cancellationToken);
            return true;
        }
    }
}