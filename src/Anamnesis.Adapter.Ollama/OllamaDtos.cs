using System.Text.Json.Serialization;

namespace Anamnesis.Adapter.Ollama;

internal record OllamaMessageDto(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content
);

internal record OllamaChatRequestDto(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] IEnumerable<OllamaMessageDto> Messages,
    [property: JsonPropertyName("stream")] bool Stream
);

internal record OllamaChatResponseDto(
    [property: JsonPropertyName("message")] OllamaMessageDto Message
);
