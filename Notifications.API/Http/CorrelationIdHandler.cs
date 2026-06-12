namespace Notifications.API.Http
{
    public class CorrelationIdHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var correlationId = _httpContextAccessor.HttpContext?
                .Items["X-Correlation-Id"]?.ToString();

            if (!string.IsNullOrEmpty(correlationId))
                request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}