using Microsoft.Extensions.Diagnostics.HealthChecks;
using Modern.Api.Api;
using Modern.Api.Application;
using Modern.Api.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Structured logging ────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, config) =>
    config
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<OrderService>();

// Use LegacyOrderAdapter instead while migrating (Strangler Fig):
// builder.Services.AddScoped<IOrderRepository, LegacyOrderAdapter>();

// ── Resilience (Polly v8) ─────────────────────────────────────────────────────
builder.Services.AddHttpClient("ExternalPaymentGateway", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["PaymentGateway:BaseUrl"] ?? "https://payments.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts    = 3;
    options.Retry.Delay               = TimeSpan.FromSeconds(1);
    options.Retry.UseJitter           = true;
    options.AttemptTimeout.Timeout    = TimeSpan.FromSeconds(10);
    options.CircuitBreaker.FailureRatio      = 0.5;
    options.CircuitBreaker.SamplingDuration  = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.BreakDuration     = TimeSpan.FromSeconds(30);
});

// ── Health checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Process is alive"))
    .AddCheck("memory", () =>
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var threshold = 512L * 1024 * 1024; // 512 MB
        return allocated < threshold
            ? HealthCheckResult.Healthy($"{allocated / 1024 / 1024} MB")
            : HealthCheckResult.Degraded($"{allocated / 1024 / 1024} MB — approaching limit");
    });

// ── OpenAPI ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (context, httpContext) =>
    {
        context.Set("RequestHost", httpContext.Request.Host.Value);
        context.Set("UserAgent",   httpContext.Request.Headers.UserAgent.ToString());
    };
});

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// ── Health endpoints ──────────────────────────────────────────────────────────
// Liveness: is the process alive? (no dependency checks)
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Name == "self"
});

// Readiness: can this instance handle traffic?
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true
});

// ── API endpoints ─────────────────────────────────────────────────────────────
app.MapOrderEndpoints();

app.Run();

// Exposed for integration tests
public partial class Program { }
