using System.Text.Json;
using Anamnesis.Adapter.Llm.Contract;
using Anamnesis.Domain;
using Anamnesis.UseCase.Conversation.Contract;

namespace Anamnesis.Adapter.Ollama;

public class OllamaConversationEngine : IConversationEngine
{
    private readonly IOllamaClient _ollamaClient;
    private readonly List<ConversationMessage> _history = [];

    public OllamaConversationEngine(IOllamaClient ollamaClient)
    {
        _ollamaClient = ollamaClient;
    }

    public async Task<string> SendMessageAsync(string userMessage)
    {
        if (_history.Count == 0)
            _history.Add(new ConversationMessage("system", PromptTemplates.MedGemmaSystemPrompt));

        _history.Add(new ConversationMessage("user", userMessage));

        try
        {
            var response = await _ollamaClient.ChatAsync(_history);
            _history.Add(new ConversationMessage("assistant", response));
            return response;
        }
        catch (RateLimitExceededException)
        {
            _history.RemoveAt(_history.Count - 1);
            return "You have sent too many messages in a short period. Please wait a moment before trying again.";
        }
        catch (LlmUnavailableException)
        {
            _history.RemoveAt(_history.Count - 1);
            throw;
        }
    }

    public async Task<bool> CheckShouldContinueAsync()
    {
        var checkMessages = new List<ConversationMessage>(_history)
        {
            new ConversationMessage("user", PromptTemplates.ContinuationCheckPrompt)
        };

        var response = await _ollamaClient.ChatAsync(checkMessages);
        return !string.Equals(response.Trim(), "END", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> SummariseAsync()
    {
        var summaryMessages = new List<ConversationMessage>(_history)
        {
            new ConversationMessage("user", PromptTemplates.SummaryPrompt)
        };

        return await _ollamaClient.ChatAsync(summaryMessages);
    }

    public async Task<SidebarResult> GetRelatedConditionsAsync(
        IReadOnlyList<RelatedCondition> conditions,
        IReadOnlyList<RelatedCondition> symptoms)
    {
        try
        {
            var conditionNames = conditions.Select(c => c.Name);
            var symptomNames = symptoms.Select(s => s.Name);

            var sidebarMessages = new List<ConversationMessage>(_history)
            {
                new ConversationMessage("user", PromptTemplates.BuildSidebarPrompt(conditionNames, symptomNames))
            };

            var response = await _ollamaClient.ChatAsync(sidebarMessages);
            return ParseSidebarResponse(response, conditions, symptoms);
        }
        catch
        {
            return new SidebarResult([], []);
        }
    }

    private static SidebarResult ParseSidebarResponse(
        string response,
        IReadOnlyList<RelatedCondition> availableConditions,
        IReadOnlyList<RelatedCondition> availableSymptoms)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd <= jsonStart)
                return new SidebarResult([], []);

            var json = response[jsonStart..(jsonEnd + 1)];
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var matchedConditions = ResolveEntries(root, "conditions", availableConditions);
            var matchedSymptoms = ResolveEntries(root, "symptoms", availableSymptoms);

            return new SidebarResult(matchedConditions, matchedSymptoms);
        }
        catch
        {
            return new SidebarResult([], []);
        }
    }

    private static IReadOnlyList<RelatedCondition> ResolveEntries(
        JsonElement root,
        string arrayProperty,
        IReadOnlyList<RelatedCondition> available)
    {
        if (!root.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
            return [];

        var lookup = available
            .Where(c => c.Url is not null)
            .ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

        var results = new List<RelatedCondition>();
        foreach (var item in array.EnumerateArray())
        {
            var name = item.GetString();
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (lookup.TryGetValue(name, out var entry))
                results.Add(entry);
        }

        return results;
    }
}
