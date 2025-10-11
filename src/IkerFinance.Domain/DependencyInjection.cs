using IkerFinance.Domain.DomainServices.Budget;
using IkerFinance.Domain.DomainServices.Transaction;
using Microsoft.Extensions.DependencyInjection;

namespace IkerFinance.Domain;

/// <summary>
/// Dependency injection configuration for Domain layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all domain services with the DI container.
    /// All domain services are registered as Transient since they're stateless.
    /// </summary>
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        // Budget domain services
        services.AddTransient<BudgetFactory>();
        services.AddTransient<BudgetUpdater>();
        services.AddTransient<BudgetCalculator>();

        // Transaction domain services
        services.AddTransient<TransactionFactory>();
        services.AddTransient<TransactionUpdater>();

        return services;
    }
}