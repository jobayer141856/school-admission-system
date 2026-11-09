using System.Reflection;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_ShouldNotReference_AnyOtherLayer()
    {
        var domainAssembly = typeof(SchoolAdmission.Domain.Common.Entity).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.DoesNotContain("SchoolAdmission.Application", referencedAssemblies);
        Assert.DoesNotContain("SchoolAdmission.Infrastructure", referencedAssemblies);
        Assert.DoesNotContain("SchoolAdmission.API", referencedAssemblies);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", referencedAssemblies);
    }

    [Fact]
    public void Application_ShouldNotReference_InfrastructureOrAPI()
    {
        var applicationAssembly = typeof(SchoolAdmission.Application.DependencyInjection).Assembly;
        var referencedAssemblies = applicationAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.DoesNotContain("SchoolAdmission.Infrastructure", referencedAssemblies);
        Assert.DoesNotContain("SchoolAdmission.API", referencedAssemblies);
    }
}

