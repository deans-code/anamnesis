using Anamnesis.Adapter.Ollama;
using Anamnesis.Adapter.Ollama.Contract;
using Anamnesis.UseCase.Conversation;
using Anamnesis.UseCase.Conversation.Contract;
using Microsoft.Extensions.Options;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddOptions<OllamaSettings>()
    .BindConfiguration("OllamaSettings")
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.Model)
            && settings.Model.StartsWith("medgemma", StringComparison.OrdinalIgnoreCase),
        "OllamaSettings:Model must start with 'medgemma'.")
    .ValidateOnStart();

var rateLimiter = new System.Threading.RateLimiting.SlidingWindowRateLimiter(
    new System.Threading.RateLimiting.SlidingWindowRateLimiterOptions
    {
        PermitLimit = 10,
        Window = TimeSpan.FromMinutes(1),
        SegmentsPerWindow = 6,
        AutoReplenishment = true,
    });

builder.Services.AddHttpClient<IOllamaClient, OllamaClient>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<OllamaSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(180);
})
.AddStandardResilienceHandler(options =>
{
    // Initial implementation by Claude Sonnet 4.6 resulted in invalid configuration.
    // Multiple attempts by the same model resolved the configuration.
    // Be aware of the capabilities of Claude Sonnet 4.6 when configuring these settings.
    options.Retry.MaxRetryAttempts = 1;
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(85);
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(180);
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
    options.CircuitBreaker.MinimumThroughput = 5;
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
    options.RateLimiter.RateLimiter = _ => rateLimiter.AcquireAsync(1);
});

builder.Services.Configure<AuditLoggingSettings>(
    builder.Configuration.GetSection("AuditLogging"));

builder.Services.AddScoped<IAuditLogger, FileAuditLogger>();
builder.Services.AddScoped<IConversationService, ConversationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<Anamnesis.Interface.Website.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
