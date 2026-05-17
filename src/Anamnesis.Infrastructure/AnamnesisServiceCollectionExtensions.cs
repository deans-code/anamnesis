using Anamnesis.Adapter.MedicalData.Contract;
using Anamnesis.Adapter.Nhs;
using Anamnesis.Adapter.Ollama;
using Anamnesis.Adapter.Llm.Contract;
using Anamnesis.UseCase.Conversation;
using Anamnesis.UseCase.Conversation.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Anamnesis.Infrastructure;

public static class AnamnesisServiceCollectionExtensions
{
    public static IServiceCollection AddAnamnesis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<OllamaSettings>()
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

        services.AddHttpClient<IOllamaClient, OllamaClient>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<OllamaSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(180);
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 1;
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(85);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(180);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
            options.CircuitBreaker.MinimumThroughput = 5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
            options.RateLimiter.RateLimiter = _ => rateLimiter.AcquireAsync(1);
        });

        services.Configure<AuditLoggingSettings>(configuration.GetSection("AuditLogging"));

        services.AddHttpClient("nhs", client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Anamnesis/1.0");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<IMedicalReferenceService>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("nhs");
            var logger = sp.GetRequiredService<ILogger<NhsConditionReferenceService>>();
            return new NhsConditionReferenceService(httpClient, logger);
        });

        services.AddScoped<IAuditLogger, FileAuditLogger>();
        services.AddScoped<IConversationEngine, OllamaConversationEngine>();
        services.AddScoped<IConversationService, ConversationService>();

        return services;
    }
}
