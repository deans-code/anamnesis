using Anamnesis.Domain;
using Anamnesis.UseCase.Conversation.Contract;

namespace Anamnesis.UseCase.Conversation;

public class ConversationService : IConversationService
{
    private readonly IConversationEngine _engine;
    private readonly IAuditLogger _auditLogger;
    private readonly INhsIndexService _nhsIndexService;
    private bool _hasStarted;
    private readonly string _sessionId = Guid.NewGuid().ToString("N");

    public ConversationService(IConversationEngine engine, IAuditLogger auditLogger, INhsIndexService nhsIndexService)
    {
        _engine = engine;
        _auditLogger = auditLogger;
        _nhsIndexService = nhsIndexService;

        _ = _auditLogger.LogAsync(new AuditEntry(
            Timestamp: DateTimeOffset.UtcNow,
            SessionId: _sessionId,
            EventType: "session_start",
            Detail: string.Empty));
    }

    public bool HasStarted => _hasStarted;

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

        var response = await _engine.SendMessageAsync(userMessage);
        _hasStarted = true;

        await _auditLogger.LogAsync(new AuditEntry(
            Timestamp: DateTimeOffset.UtcNow,
            SessionId: _sessionId,
            EventType: "llm_response",
            Detail: response));

        return response;
    }

    public async Task<bool> CheckContinuationAsync()
    {
        var shouldContinue = await _engine.CheckShouldContinueAsync();

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

        return await _engine.SummariseAsync();
    }

    public async Task<SidebarResult> GetRelatedConditionsAsync()
    {
        try
        {
            var conditionNames = await _nhsIndexService.GetNamesAsync(NhsIndexType.Conditions);
            var symptomNames = await _nhsIndexService.GetNamesAsync(NhsIndexType.Symptoms);

            var conditions = conditionNames
                .Select(n => new RelatedCondition(n, _nhsIndexService.GetUrl(n, NhsIndexType.Conditions)))
                .ToList();
            var symptoms = symptomNames
                .Select(n => new RelatedCondition(n, _nhsIndexService.GetUrl(n, NhsIndexType.Symptoms)))
                .ToList();

            return await _engine.GetRelatedConditionsAsync(conditions, symptoms);
        }
        catch
        {
            return new SidebarResult([], []);
        }
    }
}
