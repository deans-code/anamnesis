namespace Anamnesis.UseCase.Conversation.Contract;

public interface INhsIndexService
{
    Task<string?> ResolveUrlAsync(string name, IEnumerable<string> synonyms, NhsIndexType indexType);
}
