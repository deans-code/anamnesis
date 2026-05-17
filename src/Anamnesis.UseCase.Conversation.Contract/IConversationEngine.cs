using Anamnesis.Domain;

namespace Anamnesis.UseCase.Conversation.Contract;

public interface IConversationEngine
{
    Task<string> SendMessageAsync(string userMessage);

    Task<bool> CheckShouldContinueAsync();

    Task<string> SummariseAsync();

    Task<SidebarResult> GetRelatedConditionsAsync(
        IReadOnlyList<RelatedCondition> conditions,
        IReadOnlyList<RelatedCondition> symptoms);
}
