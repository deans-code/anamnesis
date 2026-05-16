namespace Anamnesis.Domain;

public record RelatedCondition(string Name, IReadOnlyList<string> Synonyms, string? Url);
