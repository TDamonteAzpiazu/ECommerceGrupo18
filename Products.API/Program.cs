using Products.API.Data;
using Products.API.ExceptionHandlers;
using Products.API.HealthChecks;
using Products.API.Repository;
using Products.API.Services;
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
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level:u3} | Products.API | {RequestMethod} | {RequestPath} | {StatusCode} | {Elapsed:0}ms | {CorrelationId}{NewLine}",
            rollingInterval: RollingInterval.Day))
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Products API",
        Version = "v1",
        Description = "API para gestión de productos del eCommerce"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// HttpClient para comunicación entre servicios
builder.Services.AddHttpClient();

// Repositorio y servicio
builder.Services.AddScoped<ProductsRepository>();
builder.Services.AddScoped<ProductsService>();

// Exception handlers
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// HealthChecks
builder.Services.AddHealthChecks()
    .AddCheck<SqliteHealthCheck>("sqlite-db", tags: new[] { "database" })
    .AddCheck<ApiStatusCheck>("api-status", tags: new[] { "api" });

builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(600);
    setup.AddHealthCheckEndpoint("Products.API", "/health");
}).AddInMemoryStorage();

// Base de datos
builder.Services.AddSingleton<DatabaseInitializer>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<Products.API.Middleware.CorrelationIdMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, _, ex) =>
        ex != null ? LogEventLevel.Error :
        httpContext.Request.Path.StartsWithSegments("/health")
            ? LogEventLevel.Verbose : LogEventLevel.Information;
});

app.UseExceptionHandler();

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

app.UseHttpsRedirection();
app.MapControllers();
Console.WriteLine("Swagger Products: https://localhost:7161/swagger");
app.Run();