using Anamnesis.Domain;

namespace Anamnesis.Adapter.Ollama;

public interface IOllamaClient
{
    Task<string> ChatAsync(IEnumerable<ConversationMessage> messages);
}
