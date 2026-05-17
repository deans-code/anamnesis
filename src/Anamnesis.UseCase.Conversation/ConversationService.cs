using System.Text.Json;
using Anamnesis.Adapter.Ollama.Contract;
using Anamnesis.Domain;
using Anamnesis.UseCase.Conversation.Contract;

namespace Anamnesis.UseCase.Conversation;

public class ConversationService : IConversationService
{
    private readonly IOllamaClient _ollamaClient;
    private readonly IAuditLogger _auditLogger;
    private readonly INhsIndexService _nhsIndexService;
    private readonly List<ConversationMessage> _history = [];
    private bool _systemPromptAdded;
    private readonly string _sessionId = Guid.NewGuid().ToString("N");

    public ConversationService(IOllamaClient ollamaClient, IAuditLogger auditLogger, INhsIndexService nhsIndexService)
    {
        _ollamaClient = ollamaClient;
        _auditLogger = auditLogger;
        _nhsIndexService = nhsIndexService;

        _ = _auditLogger.LogAsync(new AuditEntry(
            Timestamp: DateTimeOffset.UtcNow,
            SessionId: _sessionId,
            EventType: "session_start",
            Detail: string.Empty));
    }

    public bool HasStarted => _history.Count > 0;

    public async Task<string> SendAsync(string userMessage)
    {
        var decision = PolicyEvaluator.Evaluate(userMessage);
        if (decision == PolicyDecision.Deny)
        {
            await _auditLogger.LogAsync(new AuditEntry(
                Timestamp: DateTimeOffset.UtcNow,
                SessionId: _sessionId,
                EventType: "policy_deny",
                Detail: userMessage));

            return "Your request was blocked by the content policy. Please rephrase and try again.";
        }

        await _auditLogger.LogAsync(new AuditEntry(
            Timestamp: DateTimeOffset.UtcNow,
            SessionId: _sessionId,
            EventType: "user_message",
            Detail: userMessage));

        if (!_systemPromptAdded)
        {
            _history.Insert(0, new ConversationMessage("system", PromptTemplates.MedGemmaSystemPrompt));
            _systemPromptAdded = true;
        }

        _history.Add(new ConversationMessage("user", userMessage));

        try
        {
            var response = await _ollamaClient.ChatAsync(_history);
            _history.Add(new ConversationMessage("assistant", response));

            await _auditLogger.LogAsync(new AuditEntry(
                Timestamp: DateTimeOffset.UtcNow,
                SessionId: _sessionId,
                EventType: "llm_response",
                Detail: response));

            return response;
        }
        catch (RateLimitExceededException)
        {
            _history.RemoveAt(_history.Count - 1);
            return "You have sent too many messages in a short period. Please wait a moment before trying again.";
        }
        catch (OllamaUnavailableException)
        {
            _history.RemoveAt(_history.Count - 1);
            throw;
        }
    }

    public async Task<bool> CheckContinuationAsync()
    {
        var checkMessages = new List<ConversationMessage>(_history)
        {
            new ConversationMessage("user", PromptTemplates.ContinuationCheckPrompt)
        };

        var response = await _ollamaClient.ChatAsync(checkMessages);
        var trimmed = response.Trim();
        var shouldContinue = !string.Equals(trimmed, "END", StringComparison.OrdinalIgnoreCase);

        if (!shouldContinue)
        {
            await _auditLogger.LogAsync(new AuditEntry(
                Timestamp: DateTimeOffset.UtcNow,
                SessionId: _sessionId,
                EventType: "session_end",
                Detail: "LLM signalled END"));
        }

        return shouldContinue;
    }

    public async Task<string> RequestSummaryAsync()
    {
        await _auditLogger.LogAsync(new AuditEntry(
            Timestamp: DateTimeOffset.UtcNow,
            SessionId: _sessionId,
            EventType: "session_end",
            Detail: "User requested summary"));

        var summaryMessages = new List<ConversationMessage>(_history)
        {
            new ConversationMessage("user", PromptTemplates.SummaryPrompt)
        };

        return await _ollamaClient.ChatAsync(summaryMessages);
    }

    public async Task<SidebarResult> GetRelatedConditionsAsync()
    {
        try
        {
            var conditionNames = await _nhsIndexService.GetNamesAsync(NhsIndexType.Conditions);
            var symptomNames = await _nhsIndexService.GetNamesAsync(NhsIndexType.Symptoms);

            var messages = new List<ConversationMessage>(_history)
            {
                new ConversationMessage("user", PromptTemplates.BuildSidebarPrompt(conditionNames, symptomNames))
            };

            var response = await _ollamaClient.ChatAsync(messages);
            return ParseSidebarResponse(response);
        }
        catch
        {
            return new SidebarResult([], []);
        }
    }

    private SidebarResult ParseSidebarResponse(string response)
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

            var conditions = ResolveEntries(root, "conditions", NhsIndexType.Conditions);
            var symptoms = ResolveEntries(root, "symptoms", NhsIndexType.Symptoms);

            return new SidebarResult(conditions, symptoms);
        }
        catch
        {
            return new SidebarResult([], []);
        }
    }

    private IReadOnlyList<RelatedCondition> ResolveEntries(
        JsonElement root, string arrayProperty, NhsIndexType indexType)
    {
        if (!root.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
            return [];

        var results = new List<RelatedCondition>();
        foreach (var item in array.EnumerateArray())
        {
            var name = item.GetString();
            if (string.IsNullOrWhiteSpace(name)) continue;

            var url = _nhsIndexService.GetUrl(name, indexType);
            if (url is not null)
                results.Add(new RelatedCondition(name, url));
        }

        return results;
    }
}

