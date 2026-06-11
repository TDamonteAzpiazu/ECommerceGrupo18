using Orders.API.ExceptionHandlers;
using Orders.API.HealthChecks;
using Orders.API.Http;
using Orders.API.Middleware;
using Orders.API.Repository;
using Orders.API.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(le => le.Level >= LogEventLevel.Error)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(le =>
        {
            var esSerilogMiddleware = Serilog.Filters.Matching
                .FromSource("Serilog.AspNetCore.RequestLoggingMiddleware")(le);
            if (!esSerilogMiddleware) return false;
            if (le.Properties.TryGetValue("RequestPath", out var p) &&
                p is Serilog.Events.ScalarValue s && s.Value is string path)
                return !path.Contains("/health") && !path.Contains("/swagger");
            return true;
        })
        .WriteTo.File(
            path: "logs/audit.log",
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level:u3} | Orders.API | {RequestMethod} | {RequestPath} | {StatusCode} | {Elapsed:0}ms | {CorrelationId}{NewLine}",
            rollingInterval: RollingInterval.Day))
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdHandler>();
builder.Services.AddHttpClient("default")
    .AddHttpMessageHandler<CorrelationIdHandler>(); 
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddCheck<SqliteHealthCheck>("sqlite-db", tags: new[] { "database" })
    .AddCheck<ApiStatusCheck>("api-status", tags: new[] { "api" });

builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(600);
    setup.AddHealthCheckEndpoint("Orders.API", "/health");
}).AddInMemoryStorage();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, _, ex) =>
        ex != null ? LogEventLevel.Error :
        httpContext.Request.Path.StartsWithSegments("/health")
            ? LogEventLevel.Verbose : LogEventLevel.Information;
});

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("api"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");

app.MapControllers();
Console.WriteLine("Swagger Orders: https://localhost:7163/swagger");
app.Run();