using System.Net.Http.Json;
using Anamnesis.Adapter.Ollama.Contract;
using Anamnesis.Domain;
using Microsoft.Extensions.Options;

namespace Anamnesis.Adapter.Ollama;

public class OllamaClient : IOllamaClient
{
    private readonly HttpClient _httpClient;
    private readonly OllamaSettings _settings;

    public OllamaClient(HttpClient httpClient, IOptions<OllamaSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;

        if (!Uri.TryCreate(_settings.BaseUrl, UriKind.Absolute, out var baseUri)
            || (baseUri.AbsolutePath != "/" && baseUri.AbsolutePath != string.Empty))
        {
            throw new InvalidOperationException(
                $"OllamaSettings:BaseUrl '{_settings.BaseUrl}' must be a base URL with no path component. " +
                "The adapter only communicates via the /api/chat endpoint.");
        }
    }

    public async Task<string> ChatAsync(IEnumerable<ConversationMessage> messages)
    {
        var messageList = messages.ToList();
        if (messageList.Count > 200)
            throw new ArgumentException(
                $"Message history exceeds the maximum of 200 entries (was {messageList.Count}).",
                nameof(messages));

        var request = new OllamaChatRequestDto(
            Model: _settings.Model,
            Messages: messageList.Select(m => new OllamaMessageDto(m.Role, m.Content)),
            Stream: false
        );

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/chat", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<OllamaChatResponseDto>();
            return result?.Message.Content ?? string.Empty;
        }
        catch (Exception ex) when (ex.GetType().Name == "RateLimiterRejectedException")
        {
            throw new RateLimitExceededException(
                "LLM call rate limit exceeded. Please wait before sending another message.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new OllamaUnavailableException(
                "Unable to connect to Ollama. Please ensure Ollama is running with 'ollama serve'.", ex);
        }
        catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
        {
            throw new OllamaUnavailableException(
                "Connection to Ollama timed out. Please ensure Ollama is running and responsive.", ex);
        }
    }
}
