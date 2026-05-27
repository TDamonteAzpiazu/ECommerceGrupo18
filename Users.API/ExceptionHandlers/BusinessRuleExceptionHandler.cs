using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex) return false;

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
                type = context.Response.StatusCode switch
                {
                    401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
                    403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    409 => "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                },
                title = context.Response.StatusCode switch
                {
                    401 => "Unauthorized",
                    403 => "Forbidden",
                    409 => "Conflict",
                     _ => "Bad Request"
                },
                status = context.Response.StatusCode,
                detail = context.Response.StatusCode switch
                {
                    401 => "Las credenciales son incorrectas.",
                    403 => "El acceso está prohibido.",
                    409 => "Ya existe un recurso con esos datos.",
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