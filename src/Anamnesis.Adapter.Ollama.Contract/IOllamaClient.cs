using Anamnesis.Domain;

namespace Anamnesis.Adapter.Ollama.Contract;

public interface IOllamaClient
{
    Task<string> ChatAsync(IEnumerable<ConversationMessage> messages);
}
