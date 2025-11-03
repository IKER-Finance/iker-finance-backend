using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IkerFinance.Infrastructure.Mobile.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(MobileDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if already seeded
        if (await context.Currencies.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Seed Currencies
        await SeedCurrenciesAsync(context);

        // Seed Exchange Rates
        await SeedExchangeRatesAsync(context);

        // Seed System Categories
        await SeedCategoriesAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedCurrenciesAsync(MobileDbContext context)
    {
        var currencies = new List<Currency>
        {
            new Currency { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2, IsActive = true },
            new Currency { Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2, IsActive = true },
            new Currency { Code = "GBP", Name = "British Pound", Symbol = "£", DecimalPlaces = 2, IsActive = true },
            new Currency { Code = "JPY", Name = "Japanese Yen", Symbol = "¥", DecimalPlaces = 0, IsActive = true },
            new Currency { Code = "CNY", Name = "Chinese Yuan", Symbol = "¥", DecimalPlaces = 2, IsActive = true },
            new Currency { Code = "INR", Name = "Indian Rupee", Symbol = "₹", DecimalPlaces = 2, IsActive = true },
            new Currency { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", DecimalPlaces = 2, IsActive = true },
            new Currency { Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", DecimalPlaces = 2, IsActive = true },
            new Currency { Code = "CHF", Name = "Swiss Franc", Symbol = "CHF", DecimalPlaces = 2, IsActive = true },
            new Currency { Code = "SEK", Name = "Swedish Krona", Symbol = "kr", DecimalPlaces = 2, IsActive = true },
            new Currency { Code = "NOK", Name = "Norwegian Krone", Symbol = "kr", DecimalPlaces = 2, IsActive = true },
            new Currency { Code = "DKK", Name = "Danish Krone", Symbol = "kr", DecimalPlaces = 2, IsActive = true },
        };

        context.Set<Currency>().AddRange(currencies);
        await context.SaveChangesAsync();
    }

    private static async Task SeedExchangeRatesAsync(MobileDbContext context)
    {
        // Get currency IDs after seeding
        var currencies = await context.Currencies.ToDictionaryAsync(c => c.Code, c => c.Id);

        var effectiveDate = DateTime.UtcNow.Date;

        var exchangeRates = new List<ExchangeRate>
        {
            // USD as base
            new ExchangeRate { FromCurrencyId = currencies["USD"], ToCurrencyId = currencies["EUR"], Rate = 0.92m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["USD"], ToCurrencyId = currencies["GBP"], Rate = 0.79m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["USD"], ToCurrencyId = currencies["JPY"], Rate = 149.50m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["USD"], ToCurrencyId = currencies["CNY"], Rate = 7.24m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["USD"], ToCurrencyId = currencies["INR"], Rate = 83.20m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["USD"], ToCurrencyId = currencies["AUD"], Rate = 1.53m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["USD"], ToCurrencyId = currencies["CAD"], Rate = 1.36m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["USD"], ToCurrencyId = currencies["CHF"], Rate = 0.88m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["USD"], ToCurrencyId = currencies["SEK"], Rate = 10.85m, EffectiveDate = effectiveDate, IsActive = true },

            // EUR as base
            new ExchangeRate { FromCurrencyId = currencies["EUR"], ToCurrencyId = currencies["USD"], Rate = 1.09m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["EUR"], ToCurrencyId = currencies["GBP"], Rate = 0.86m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["EUR"], ToCurrencyId = currencies["SEK"], Rate = 11.80m, EffectiveDate = effectiveDate, IsActive = true },

            // GBP as base
            new ExchangeRate { FromCurrencyId = currencies["GBP"], ToCurrencyId = currencies["USD"], Rate = 1.27m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["GBP"], ToCurrencyId = currencies["EUR"], Rate = 1.16m, EffectiveDate = effectiveDate, IsActive = true },

            // SEK as base (Swedish Krona - common for BTH students)
            new ExchangeRate { FromCurrencyId = currencies["SEK"], ToCurrencyId = currencies["USD"], Rate = 0.092m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["SEK"], ToCurrencyId = currencies["EUR"], Rate = 0.085m, EffectiveDate = effectiveDate, IsActive = true },
            new ExchangeRate { FromCurrencyId = currencies["SEK"], ToCurrencyId = currencies["GBP"], Rate = 0.073m, EffectiveDate = effectiveDate, IsActive = true },
        };

        context.Set<ExchangeRate>().AddRange(exchangeRates);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(MobileDbContext context)
    {
        var systemCategories = new List<Category>
        {
            // Expense Categories (TransactionType enum only has Expense)
            new Category { Name = "Food & Dining", Description = "Groceries and restaurants", Icon = "🍔", Color = "#F44336", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 1 },
            new Category { Name = "Transportation", Description = "Public transport, fuel, car maintenance", Icon = "🚗", Color = "#E91E63", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 12 },
            new Category { Name = "Housing", Description = "Rent, mortgage, utilities", Icon = "🏠", Color = "#9C27B0", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 13 },
            new Category { Name = "Entertainment", Description = "Movies, games, hobbies", Icon = "🎮", Color = "#673AB7", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 14 },
            new Category { Name = "Shopping", Description = "Clothing, electronics, general shopping", Icon = "🛍️", Color = "#3F51B5", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 15 },
            new Category { Name = "Healthcare", Description = "Medical expenses, pharmacy", Icon = "🏥", Color = "#2196F3", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 16 },
            new Category { Name = "Education", Description = "Tuition, books, courses", Icon = "📚", Color = "#03A9F4", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 17 },
            new Category { Name = "Bills & Utilities", Description = "Electricity, water, internet", Icon = "📄", Color = "#00BCD4", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 18 },
            new Category { Name = "Travel", Description = "Vacation, trips", Icon = "✈️", Color = "#009688", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 19 },
            new Category { Name = "Personal Care", Description = "Haircut, cosmetics", Icon = "💅", Color = "#4CAF50", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 20 },
            new Category { Name = "Insurance", Description = "Health, car, life insurance", Icon = "🛡️", Color = "#8BC34A", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 21 },
            new Category { Name = "Subscriptions", Description = "Streaming, software, memberships", Icon = "📱", Color = "#CDDC39", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 22 },
            new Category { Name = "Other Expenses", Description = "Miscellaneous expenses", Icon = "💸", Color = "#FFC107", Type = TransactionType.Expense, IsSystem = true, IsActive = true, SortOrder = 23 },
        };

        context.Set<Category>().AddRange(systemCategories);
        await context.SaveChangesAsync();
    }
}
