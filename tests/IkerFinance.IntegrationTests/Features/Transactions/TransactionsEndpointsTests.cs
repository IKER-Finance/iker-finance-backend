using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IkerFinance.Application.Features.Transactions.Commands.CreateTransaction;
using IkerFinance.Application.Features.Transactions.Commands.UpdateTransaction;
using IkerFinance.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IkerFinance.IntegrationTests.Features.Transactions;

public class TransactionsEndpointsTests : BaseIntegrationTest
{
    public TransactionsEndpointsTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateTransaction_WithValidData_ShouldReturnCreated()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            CurrencyId = currency.Id,
            CategoryId = category.Id,
            Date = DateTime.UtcNow,
            Description = "Test transaction",
            Notes = "Test notes"
        };

        var response = await PostAsync("/api/transactions", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTransaction_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test transaction"
        };

        var response = await PostAsync("/api/transactions", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTransactions_WithAuthentication_ShouldReturnSuccess()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var response = await Client.GetAsync("/api/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTransactionById_WithValidId_ShouldReturnTransaction()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        var createCommand = new CreateTransactionCommand
        {
            Amount = 75.00m,
            CurrencyId = currency.Id,
            CategoryId = category.Id,
            Date = DateTime.UtcNow,
            Description = "Get by ID test"
        };

        var createResponse = await PostAsync("/api/transactions", createCommand);
        var createdTransaction = await createResponse.Content.ReadFromJsonAsync<TransactionResponse>();

        var response = await Client.GetAsync($"/api/transactions/{createdTransaction!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateTransaction_WithValidData_ShouldReturnSuccess()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        var createCommand = new CreateTransactionCommand
        {
            Amount = 50.00m,
            CurrencyId = currency.Id,
            CategoryId = category.Id,
            Date = DateTime.UtcNow,
            Description = "Original description"
        };

        var createResponse = await PostAsync("/api/transactions", createCommand);
        var createdTransaction = await createResponse.Content.ReadFromJsonAsync<TransactionResponse>();

        var updateCommand = new UpdateTransactionCommand
        {
            Amount = 60.00m,
            CurrencyId = currency.Id,
            CategoryId = category.Id,
            Date = DateTime.UtcNow,
            Description = "Updated description"
        };

        var response = await PutAsync($"/api/transactions/{createdTransaction!.Id}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteTransaction_WithValidId_ShouldReturnNoContent()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        var createCommand = new CreateTransactionCommand
        {
            Amount = 25.00m,
            CurrencyId = currency.Id,
            CategoryId = category.Id,
            Date = DateTime.UtcNow,
            Description = "To be deleted"
        };

        var createResponse = await PostAsync("/api/transactions", createCommand);
        var createdTransaction = await createResponse.Content.ReadFromJsonAsync<TransactionResponse>();

        var response = await DeleteAsync($"/api/transactions/{createdTransaction!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetTransactionSummary_WithAuthentication_ShouldReturnSummary()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var response = await Client.GetAsync("/api/transactions/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTransactions_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await Client.GetAsync("/api/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Test: Currency conversion works end-to-end (UC2 Step 6, Extension 6a)
    [Fact]
    public async Task CreateTransaction_WithDifferentCurrency_ShouldConvertToHomeCurrency()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        // Get currencies - assuming USD and EUR exist in seed data
        var usd = await DbContext.Currencies.FirstOrDefaultAsync(c => c.Code == "USD");
        var eur = await DbContext.Currencies.FirstOrDefaultAsync(c => c.Code == "EUR");
        var category = DbContext.Categories.First();

        // Skip test if currencies don't exist
        if (usd == null || eur == null)
        {
            return;
        }

        // Create exchange rate: 1 EUR = 1.1 USD
        var exchangeRate = new IkerFinance.Domain.Entities.ExchangeRate
        {
            FromCurrencyId = eur.Id,
            ToCurrencyId = usd.Id,
            Rate = 1.1m,
            IsActive = true,
            EffectiveDate = DateTime.UtcNow
        };
        await DbContext.Set<IkerFinance.Domain.Entities.ExchangeRate>().AddAsync(exchangeRate);
        await DbContext.SaveChangesAsync();

        // Create transaction in EUR
        var command = new CreateTransactionCommand
        {
            Amount = 100m,
            CurrencyId = eur.Id,
            CategoryId = category.Id,
            Date = DateTime.UtcNow,
            Description = "Test EUR transaction"
        };

        var response = await PostAsync("/api/transactions", command);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTransaction = await response.Content.ReadFromJsonAsync<TransactionDetailResponse>();

        // Verify conversion happened
        createdTransaction.Should().NotBeNull();
        createdTransaction!.Amount.Should().Be(100m); // Original amount
        createdTransaction.ConvertedAmount.Should().Be(110m); // Converted amount
        createdTransaction.ExchangeRate.Should().Be(1.1m);
        createdTransaction.CurrencyCode.Should().Be("EUR");
        createdTransaction.ConvertedCurrencyCode.Should().Be("USD");
    }

    private class TransactionResponse
    {
        public int Id { get; set; }
    }

    private class TransactionDetailResponse
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string ConvertedCurrencyCode { get; set; } = string.Empty;
    }
}
