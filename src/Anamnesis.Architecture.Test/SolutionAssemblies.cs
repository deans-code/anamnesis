using System.Reflection;
using Anamnesis.Domain;
using Anamnesis.UseCase.Conversation;
using Anamnesis.UseCase.Conversation.Contract;
using Anamnesis.Adapter.Llm.Contract;
using Anamnesis.Adapter.MedicalData.Contract;
using Anamnesis.Adapter.Llm.Ollama;
using Anamnesis.Adapter.MedicalData.Nhs;
using Anamnesis.Infrastructure;

namespace Anamnesis.Architecture.Test;

internal static class SolutionAssemblies
{
    private static readonly string OutputDir =
        Path.GetDirectoryName(typeof(ConversationMessage).Assembly.Location)!;

    internal static readonly Assembly Domain =
        typeof(ConversationMessage).Assembly;
    internal static readonly Assembly UseCaseConversation =
        typeof(ConversationService).Assembly;
    internal static readonly Assembly UseCaseConversationContract =
        typeof(IConversationService).Assembly;
    internal static readonly Assembly AdapterLlmContract =
        typeof(LlmUnavailableException).Assembly;
    internal static readonly Assembly AdapterMedicalDataContract =
        typeof(IMedicalReferenceService).Assembly;
    internal static readonly Assembly AdapterLlmOllama =
        typeof(OllamaSettings).Assembly;
    internal static readonly Assembly AdapterMedicalDataNhs =
        typeof(NhsConditionReferenceService).Assembly;
    internal static readonly Assembly Infrastructure =
        typeof(AnamnesisServiceCollectionExtensions).Assembly;
    internal static readonly Assembly InterfaceWebsite =
        Assembly.LoadFrom(Path.Combine(OutputDir, "Anamnesis.Interface.Website.dll"));

    internal static readonly Assembly[] AllProduction =
    [
        Domain,
        UseCaseConversation,
        UseCaseConversationContract,
        AdapterLlmContract,
        AdapterMedicalDataContract,
        AdapterLlmOllama,
        AdapterMedicalDataNhs,
        Infrastructure,
        InterfaceWebsite,
    ];

    internal static readonly Assembly[] UseCaseImplementations = [UseCaseConversation];
    internal static readonly Assembly[] UseCaseContracts = [UseCaseConversationContract];
    internal static readonly Assembly[] AdapterImplementations = [AdapterLlmOllama, AdapterMedicalDataNhs];
    internal static readonly Assembly[] AdapterContracts = [AdapterLlmContract, AdapterMedicalDataContract];

    internal static IEnumerable<string> GetAnamnesisRefs(Assembly a) =>
        a.GetReferencedAssemblies()
         .Where(r => r.Name!.StartsWith("Anamnesis.", StringComparison.Ordinal))
         .Select(r => r.Name!);

    internal static IEnumerable<Assembly> LoadAllInOutputDir() =>
        Directory.GetFiles(OutputDir, "Anamnesis.*.dll")
            .Select(Assembly.LoadFrom);
}
