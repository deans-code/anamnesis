namespace Anamnesis.UseCase.Conversation.Contract;

public interface IConversationService
{
    bool HasStarted { get; }

    Task<string> SendAsync(string userMessage);

    Task<bool> CheckContinuationAsync();

    Task<string> RequestSummaryAsync();
}
