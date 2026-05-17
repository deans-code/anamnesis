using Anamnesis.Domain;

namespace Anamnesis.Adapter.MedicalData.Contract;

public interface IMedicalReferenceService
{
    Task<IReadOnlyList<RelatedCondition>> GetConditionsAsync();

    Task<IReadOnlyList<RelatedCondition>> GetSymptomsAsync();
}
