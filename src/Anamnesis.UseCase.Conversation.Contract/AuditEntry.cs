namespace Anamnesis.UseCase.Conversation.Contract;

public record AuditEntry(
    DateTimeOffset Timestamp,
    string SessionId,
    string EventType,
    string Detail);
