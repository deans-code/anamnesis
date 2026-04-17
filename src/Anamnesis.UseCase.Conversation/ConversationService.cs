using Anamnesis.Adapter.Ollama.Contract;
using Anamnesis.Domain;
using Anamnesis.UseCase.Conversation.Contract;

namespace Anamnesis.UseCase.Conversation;

public class ConversationService : IConversationService
{
    private readonly IOllamaClient _ollamaClient;
    private readonly List<ConversationMessage> _history = [];
    private bool _systemPromptAdded;

    public ConversationService(IOllamaClient ollamaClient)
    {
        _ollamaClient = ollamaClient;
    }

    public bool HasStarted => _history.Count > 0;

    public async Task<string> SendAsync(string userMessage)
    {
        if (!_systemPromptAdded)
        {
            _history.Insert(0, new ConversationMessage("system", PromptTemplates.MedGemmaSystemPrompt));
            _systemPromptAdded = true;
        }

        _history.Add(new ConversationMessage("user", userMessage));

        var response = await _ollamaClient.ChatAsync(_history);
        _history.Add(new ConversationMessage("assistant", response));

        return response;
    }

    public async Task<bool> CheckContinuationAsync()
    {
        var checkMessages = new List<ConversationMessage>(_history)
        {
            new ConversationMessage("user", PromptTemplates.ContinuationCheckPrompt)
        };

        var response = await _ollamaClient.ChatAsync(checkMessages);
        return !response.Trim().Contains("END", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> RequestSummaryAsync()
    {
        var summaryMessages = new List<ConversationMessage>(_history)
        {
            new ConversationMessage("user", PromptTemplates.SummaryPrompt)
        };

        return await _ollamaClient.ChatAsync(summaryMessages);
    }
}
