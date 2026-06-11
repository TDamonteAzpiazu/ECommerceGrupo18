using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Orders.API.HealthChecks
{
    public class ApiStatusCheck : IHealthCheck
    {
        private readonly IWebHostEnvironment _env;
        private static readonly DateTime _startTime = DateTime.UtcNow;

        public ApiStatusCheck(IWebHostEnvironment env) => _env = env;

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var uptime = DateTime.UtcNow - _startTime;
            var data = new Dictionary<string, object>
            {
                { "uptime", uptime.ToString(@"hh\:mm\:ss") },
                { "environment", _env.EnvironmentName },
                { "dotnetVersion", Environment.Version.ToString() },
                { "startTime", _startTime.ToString("o") }
            };
            return Task.FromResult(HealthCheckResult.Healthy("API operativa", data));
        }
    }
}