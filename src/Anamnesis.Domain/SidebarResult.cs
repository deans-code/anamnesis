namespace Anamnesis.Domain;

public record SidebarResult(
    IReadOnlyList<RelatedCondition> Conditions,
    IReadOnlyList<RelatedCondition> Symptoms);
