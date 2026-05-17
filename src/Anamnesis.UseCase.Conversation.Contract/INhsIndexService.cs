namespace Anamnesis.UseCase.Conversation.Contract;

public interface INhsIndexService
{
    Task<IReadOnlyList<string>> GetNamesAsync(NhsIndexType indexType);
    string? GetUrl(string exactName, NhsIndexType indexType);
}
