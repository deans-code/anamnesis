using System.Reflection;
using Xunit;

namespace Anamnesis.Architecture.Test;

public class LayerDependencyTests
{
    [Fact]
    public void Domain_HasNoAnamnesisReferences()
    {
        var refs = SolutionAssemblies.GetAnamnesisRefs(SolutionAssemblies.Domain);
        Assert.Empty(refs);
    }

    [Fact]
    public void UseCaseContracts_DoNotReferenceUseCaseImplementationsOrAdapters()
    {
        foreach (var assembly in SolutionAssemblies.UseCaseContracts)
        {
            var refs = SolutionAssemblies.GetAnamnesisRefs(assembly).ToList();

            var useCaseImplRefs = refs.Where(r =>
                SolutionAssemblies.UseCaseImplementations.Any(a => a.GetName().Name == r));
            Assert.Empty(useCaseImplRefs);

            var adapterRefs = refs.Where(r => r.StartsWith("Anamnesis.Adapter.", StringComparison.Ordinal));
            Assert.Empty(adapterRefs);
        }
    }

    [Fact]
    public void UseCaseImplementations_DoNotReferenceAdapterImplementationsInfrastructureOrInterface()
    {
        foreach (var assembly in SolutionAssemblies.UseCaseImplementations)
        {
            var refs = SolutionAssemblies.GetAnamnesisRefs(assembly).ToList();

            var adapterImplRefs = refs.Where(r =>
                SolutionAssemblies.AdapterImplementations.Any(a => a.GetName().Name == r));
            Assert.Empty(adapterImplRefs);

            var infraRefs = refs.Where(r => r == SolutionAssemblies.Infrastructure.GetName().Name);
            Assert.Empty(infraRefs);

            var interfaceRefs = refs.Where(r => r.StartsWith("Anamnesis.Interface.", StringComparison.Ordinal));
            Assert.Empty(interfaceRefs);
        }
    }

    [Fact]
    public void AdapterContracts_DoNotReferenceUseCaseOrAdapterImplementations()
    {
        foreach (var assembly in SolutionAssemblies.AdapterContracts)
        {
            var refs = SolutionAssemblies.GetAnamnesisRefs(assembly).ToList();

            var useCaseRefs = refs.Where(r => r.StartsWith("Anamnesis.UseCase.", StringComparison.Ordinal));
            Assert.Empty(useCaseRefs);

            var adapterImplRefs = refs.Where(r =>
                SolutionAssemblies.AdapterImplementations.Any(a => a.GetName().Name == r));
            Assert.Empty(adapterImplRefs);
        }
    }

    [Fact]
    public void AdapterImplementations_DoNotReferenceUseCaseImplementationsInfrastructureInterfaceOrSiblingAdapters()
    {
        foreach (var assembly in SolutionAssemblies.AdapterImplementations)
        {
            var refs = SolutionAssemblies.GetAnamnesisRefs(assembly).ToList();

            var useCaseImplRefs = refs.Where(r =>
                SolutionAssemblies.UseCaseImplementations.Any(a => a.GetName().Name == r));
            Assert.Empty(useCaseImplRefs);

            var infraRefs = refs.Where(r => r == SolutionAssemblies.Infrastructure.GetName().Name);
            Assert.Empty(infraRefs);

            var interfaceRefs = refs.Where(r => r.StartsWith("Anamnesis.Interface.", StringComparison.Ordinal));
            Assert.Empty(interfaceRefs);

            var siblingAdapterRefs = refs.Where(r =>
                SolutionAssemblies.AdapterImplementations
                    .Where(a => a != assembly)
                    .Any(a => a.GetName().Name == r));
            Assert.Empty(siblingAdapterRefs);
        }
    }

    [Fact]
    public void OnlyInfrastructure_ReferencesBothUseCaseAndAdapterImplementations()
    {
        foreach (var assembly in SolutionAssemblies.AllProduction)
        {
            if (assembly == SolutionAssemblies.Infrastructure)
                continue;

            var refs = SolutionAssemblies.GetAnamnesisRefs(assembly).ToList();

            bool refsUseCaseImpl = refs.Any(r =>
                SolutionAssemblies.UseCaseImplementations.Any(a => a.GetName().Name == r));
            bool refsAdapterImpl = refs.Any(r =>
                SolutionAssemblies.AdapterImplementations.Any(a => a.GetName().Name == r));

            Assert.False(refsUseCaseImpl && refsAdapterImpl,
                $"{assembly.GetName().Name} references both UseCase and Adapter implementations, but only Infrastructure is allowed to.");
        }
    }
}
