namespace Anamnesis.UseCase.Conversation.Contract;

public interface IAuditLogger
{
    Task LogAsync(AuditEntry entry);
}
