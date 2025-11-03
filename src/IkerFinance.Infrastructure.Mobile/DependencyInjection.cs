using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Infrastructure.Mobile.Data;
using IkerFinance.Infrastructure.Mobile.Repositories;
using IkerFinance.Infrastructure.Mobile.Services;

namespace IkerFinance.Infrastructure.Mobile;

public static class DependencyInjection
{
    public static IServiceCollection AddMobileInfrastructure(
        this IServiceCollection services,
        string databasePath)
    {
        // Register DbContext with SQLite
        services.AddDbContext<MobileDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        // Register DbContext as IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<MobileDbContext>());

        // Register Repositories
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();

        // Register Services
        services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();
        services.AddSingleton<IMobileAuthService, MobileAuthService>();

        return services;
    }
}
