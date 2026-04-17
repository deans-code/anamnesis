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

builder.Services.Configure<OllamaSettings>(
    builder.Configuration.GetSection("OllamaSettings"));

builder.Services.AddHttpClient<IOllamaClient, OllamaClient>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<OllamaSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(120);
});

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
