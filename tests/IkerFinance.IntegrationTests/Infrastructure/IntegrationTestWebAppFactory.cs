using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IkerFinance.Infrastructure.Data;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;

namespace IkerFinance.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=ikerfinance_test;Username=mdasifiqbalahmed",
                ["JwtSettings:SecretKey"] = "ThisIsATestSecretKeyForIntegrationTests12345",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience"
            }!);
        });

        builder.UseEnvironment("Testing");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            SeedTestData(db);
        }

        return host;
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        var currencies = new List<Currency>
        {
            new() { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2 },
            new() { Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2 },
            new() { Code = "SEK", Name = "Swedish Krona", Symbol = "kr", DecimalPlaces = 2 }
        };

        foreach (var currency in currencies)
        {
            context.Add(currency);
        }

        context.SaveChanges();

        var categories = new List<Category>
        {
            new() { Name = "Food", Icon = "utensils", Color = "#FF6B6B", Type = TransactionType.Expense, IsSystem = true, SortOrder = 1 },
            new() { Name = "Transport", Icon = "car", Color = "#4ECDC4", Type = TransactionType.Expense, IsSystem = true, SortOrder = 2 },
            new() { Name = "Shopping", Icon = "shopping-bag", Color = "#45B7D1", Type = TransactionType.Expense, IsSystem = true, SortOrder = 3 }
        };

        foreach (var category in categories)
        {
            context.Add(category);
        }

        context.SaveChanges();
    }
}
