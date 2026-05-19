using Xunit;

namespace Anamnesis.Architecture.Test;

public class NamespaceConventionTests
{
    [Fact]
    public void AllProductionTypes_HaveNamespacesStartingWithAssemblyName()
    {
        foreach (var assembly in SolutionAssemblies.AllProduction)
        {
            var assemblyName = assembly.GetName().Name!;
            var violations = assembly.GetTypes()
                .Where(t => t.Namespace is not null)
                .Where(t => t.Namespace!.StartsWith("Anamnesis.", StringComparison.Ordinal))
                .Where(t => !t.Namespace!.StartsWith(assemblyName, StringComparison.Ordinal))
                .Select(t => $"{t.FullName} (namespace: {t.Namespace})")
                .ToList();

            Assert.Empty(violations);
        }
    }
}
