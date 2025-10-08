using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedCurrenciesAsync(context);
        await SeedCategoriesAsync(context);
        await SeedRolesAsync(roleManager);
        await SeedTestUsersAsync(userManager, context);
    }

    private static async Task SeedCurrenciesAsync(ApplicationDbContext context)
    {
        if (context.Currencies.Any()) return;

        var currencies = new List<Currency>
        {
            new() { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2 },
            new() { Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2 },
            new() { Code = "SEK", Name = "Swedish Krona", Symbol = "kr", DecimalPlaces = 2 },
            new() { Code = "NGN", Name = "Nigerian Naira", Symbol = "₦", DecimalPlaces = 2 },
            new() { Code = "BDT", Name = "Bangladeshi Taka", Symbol = "৳", DecimalPlaces = 2 },
            new() { Code = "CNY", Name = "Chinese Yuan", Symbol = "¥", DecimalPlaces = 2 }
        };

        context.Currencies.AddRange(currencies);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (context.Categories.Any()) return;

        var categories = new List<Category>
        {
            new() { Name = "Food", Icon = "utensils", Color = "#FF6B6B", Type = TransactionType.Expense, IsSystem = true, SortOrder = 1 },
            new() { Name = "Transport", Icon = "car", Color = "#4ECDC4", Type = TransactionType.Expense, IsSystem = true, SortOrder = 2 },
            new() { Name = "Shopping", Icon = "shopping-bag", Color = "#45B7D1", Type = TransactionType.Expense, IsSystem = true, SortOrder = 3 },
            new() { Name = "Bills", Icon = "file-text", Color = "#96CEB4", Type = TransactionType.Expense, IsSystem = true, SortOrder = 4 }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "SystemAdmin", "MultiCurrencyUser" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task SeedTestUsersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        var adminEmail = "admin@ikerfinance.com";
        if (await userManager.FindByEmailAsync(adminEmail) != null) return;

        var sekCurrency = context.Currencies.First(c => c.Code == "SEK");

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Admin",
            HomeCurrencyId = sekCurrency.Id,
            DefaultTransactionCurrencyId = sekCurrency.Id,
            PreferredLanguage = "en",
            RegistrationDate = DateTime.UtcNow,
            EmailConfirmed = true,
            IsActive = true,
            CurrencySetupComplete = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123456");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "SystemAdmin");
        }
    }
}