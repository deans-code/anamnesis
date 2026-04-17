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
    }

    public async Task<string> ChatAsync(IEnumerable<ConversationMessage> messages)
    {
        var request = new OllamaChatRequestDto(
            Model: _settings.Model,
            Messages: messages.Select(m => new OllamaMessageDto(m.Role, m.Content)),
            Stream: false
        );

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/chat", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<OllamaChatResponseDto>();
            return result?.Message.Content ?? string.Empty;
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
