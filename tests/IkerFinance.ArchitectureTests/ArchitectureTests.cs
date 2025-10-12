using NetArchTest.Rules;
using System.Reflection;

namespace IkerFinance.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(Domain.Common.BaseEntity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.Common.Interfaces.IApplicationDbContext).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.Data.ApplicationDbContext).Assembly;

    [Fact]
    public void Domain_Should_Not_HaveDependencyOnOtherLayers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("IkerFinance.Application")
            .And()
            .NotHaveDependencyOn("IkerFinance.Infrastructure")
            .And()
            .NotHaveDependencyOn("IkerFinance.API")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain layer should not depend on other layers");
    }

    [Fact]
    public void Domain_Should_Not_HaveDependencyOnFrameworks()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .And()
            .NotHaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain layer should not depend on framework packages");
    }

    [Fact]
    public void Application_Should_OnlyDependOnDomain()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("IkerFinance.Infrastructure")
            .And()
            .NotHaveDependencyOn("IkerFinance.API")
            .GetResult();

        Assert.True(result.IsSuccessful, "Application layer should only depend on Domain");
    }

    [Fact]
    public void Infrastructure_Should_NotDependOnAPI()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("IkerFinance.API")
            .GetResult();

        Assert.True(result.IsSuccessful, "Infrastructure should not depend on API layer");
    }

    [Fact]
    public void Controllers_Should_HaveDependencyOnMediatR()
    {
        var apiAssembly = typeof(API.Program).Assembly;

        var result = Types.InAssembly(apiAssembly)
            .That()
            .ResideInNamespace("IkerFinance.API.Controllers")
            .Should()
            .HaveDependencyOn("MediatR")
            .GetResult();

        Assert.True(result.IsSuccessful, "Controllers should use MediatR for CQRS");
    }

    [Fact]
    public void Handlers_Should_BeSealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, "Handlers should be sealed for performance");
    }

    [Fact]
    public void DomainServices_Should_BeInDomainServicesNamespace()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameEndingWith("Factory")
            .Or()
            .HaveNameEndingWith("Updater")
            .Or()
            .HaveNameEndingWith("Calculator")
            .Should()
            .ResideInNamespace("IkerFinance.Domain.DomainServices")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain services should be in DomainServices namespace");
    }

    [Fact]
    public void Entities_Should_InheritFromBaseEntity()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespace("IkerFinance.Domain.Entities")
            .And()
            .AreClasses()
            .Should()
            .Inherit(typeof(Domain.Common.BaseEntity))
            .GetResult();

        Assert.True(result.IsSuccessful, "All entities should inherit from BaseEntity");
    }

    [Fact]
    public void Commands_Should_BeInCommandsNamespace()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Command")
            .Should()
            .ResideInNamespace("IkerFinance.Application.Features")
            .GetResult();

        Assert.True(result.IsSuccessful, "Commands should be in Features namespace");
    }

    [Fact]
    public void Queries_Should_BeInQueriesNamespace()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Query")
            .Should()
            .ResideInNamespace("IkerFinance.Application.Features")
            .GetResult();

        Assert.True(result.IsSuccessful, "Queries should be in Features namespace");
    }
}
