using System.Reflection;
using Xunit;

namespace Anamnesis.Architecture.Test;

public class AdapterTrioTests
{
    private static readonly IReadOnlyList<string> AdapterAssemblyNames =
        SolutionAssemblies.LoadAllInOutputDir()
            .Select(a => a.GetName().Name!)
            .Where(n => n.StartsWith("Anamnesis.Adapter.", StringComparison.Ordinal))
            .ToList();

    private static string? DomainConcept(string name)
    {
        var parts = name.Split('.');
        return parts.Length >= 4 ? parts[2] : null;
    }

    private static bool IsContract(string name) =>
        name.Split('.') is { Length: 4 } p && p[3] == "Contract";

    private static bool IsImplementation(string name) =>
        name.Split('.') is { Length: 4 } p && p[3] != "Contract" && p[3] != "Test";

    private static bool IsTestAssembly(string name) =>
        name.Split('.') is { Length: 5 } p && p[4] == "Test";

    [Fact]
    public void EachAdapterContractHasAtLeastOneImplementation()
    {
        var contractConcepts = AdapterAssemblyNames.Where(IsContract).Select(DomainConcept).ToHashSet();
        var implementationConcepts = AdapterAssemblyNames.Where(IsImplementation).Select(DomainConcept).ToHashSet();

        var missing = contractConcepts.Where(c => !implementationConcepts.Contains(c)).ToList();
        Assert.Empty(missing);
    }

    [Fact]
    public void EachAdapterImplementationHasAContract()
    {
        var contractConcepts = AdapterAssemblyNames.Where(IsContract).Select(DomainConcept).ToHashSet();
        var implementations = AdapterAssemblyNames.Where(IsImplementation).ToList();

        var missing = implementations
            .Where(n => !contractConcepts.Contains(DomainConcept(n)))
            .ToList();
        Assert.Empty(missing);
    }

    [Fact]
    public void EachAdapterImplementationHasAMatchingTestAssembly()
    {
        var implementations = AdapterAssemblyNames.Where(IsImplementation).ToList();

        var missing = implementations
            .Where(n => !AdapterAssemblyNames.Contains(n + ".Test"))
            .ToList();
        Assert.Empty(missing);
    }
}
