using IkerFinance.Domain.DomainServices.Budget;
using IkerFinance.Domain.DomainServices.Transaction;
using Microsoft.Extensions.DependencyInjection;

namespace IkerFinance.Domain;

/// <summary>
/// Dependency injection configuration for Domain layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddTransient<BudgetService>();
        services.AddTransient<BudgetCalculator>();
        services.AddTransient<TransactionService>();

        return services;
    }
}