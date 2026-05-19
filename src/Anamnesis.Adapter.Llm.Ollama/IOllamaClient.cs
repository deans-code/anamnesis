using Anamnesis.Domain;

namespace Anamnesis.Adapter.Llm.Ollama;

public interface IOllamaClient
{
    Task<string> ChatAsync(IEnumerable<ConversationMessage> messages);
}
