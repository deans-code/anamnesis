using System.Reflection;
using Xunit;

namespace Anamnesis.Architecture.Test;

public class NamingConventionTests
{
    [Fact]
    public void AdapterContractAssemblies_HaveExactlyFourSegments()
    {
        foreach (var assembly in SolutionAssemblies.AdapterContracts)
        {
            var name = assembly.GetName().Name!;
            Assert.Equal(4, name.Split('.').Length);
        }
    }

    [Fact]
    public void AdapterImplementationAssemblies_HaveExactlyFourSegments()
    {
        foreach (var assembly in SolutionAssemblies.AdapterImplementations)
        {
            var name = assembly.GetName().Name!;
            Assert.Equal(4, name.Split('.').Length);
        }
    }

    [Fact]
    public void UseCaseImplementationAssemblies_HaveExactlyThreeSegments()
    {
        foreach (var assembly in SolutionAssemblies.UseCaseImplementations)
        {
            var name = assembly.GetName().Name!;
            Assert.Equal(3, name.Split('.').Length);
        }
    }

    [Fact]
    public void UseCaseContractAssemblies_HaveExactlyFourSegments()
    {
        foreach (var assembly in SolutionAssemblies.UseCaseContracts)
        {
            var name = assembly.GetName().Name!;
            Assert.Equal(4, name.Split('.').Length);
        }
    }

    [Fact]
    public void TestProjectAssemblies_HaveNameEndingInDotTest()
    {
        foreach (var assembly in SolutionAssemblies.LoadAllInOutputDir())
        {
            bool hasTests = assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Any(m => m.GetCustomAttributes()
                    .Any(a => a.GetType().FullName == "Xunit.FactAttribute"
                           || a.GetType().FullName == "Xunit.TheoryAttribute"));

            if (hasTests)
                Assert.EndsWith(".Test", assembly.GetName().Name, StringComparison.Ordinal);
        }
    }
}
