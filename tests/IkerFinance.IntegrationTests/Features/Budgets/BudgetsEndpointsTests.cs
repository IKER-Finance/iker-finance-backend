using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IkerFinance.Application.Features.Budgets.Commands.CreateBudget;
using IkerFinance.Application.Features.Budgets.Commands.UpdateBudget;
using IkerFinance.Domain.Enums;
using IkerFinance.IntegrationTests.Infrastructure;

namespace IkerFinance.IntegrationTests.Features.Budgets;

public class BudgetsEndpointsTests : BaseIntegrationTest
{
    public BudgetsEndpointsTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateBudget_WithValidData_ShouldReturnCreated()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        var command = new CreateBudgetCommand
        {
            CategoryId = category.Id,
            CurrencyId = currency.Id,
            Amount = 1000.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = "Test budget",
            IsActive = true
        };

        var response = await PostAsync("/api/budgets", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateBudget_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var command = new CreateBudgetCommand
        {
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var response = await PostAsync("/api/budgets", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBudgets_WithAuthentication_ShouldReturnSuccess()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var response = await Client.GetAsync("/api/budgets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBudgetById_WithValidId_ShouldReturnBudget()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        var createCommand = new CreateBudgetCommand
        {
            CategoryId = category.Id,
            CurrencyId = currency.Id,
            Amount = 500.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = "Get by ID test"
        };

        var createResponse = await PostAsync("/api/budgets", createCommand);
        var createdBudget = await createResponse.Content.ReadFromJsonAsync<BudgetResponse>();

        var response = await Client.GetAsync($"/api/budgets/{createdBudget!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateBudget_WithValidData_ShouldReturnSuccess()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        var createCommand = new CreateBudgetCommand
        {
            CategoryId = category.Id,
            CurrencyId = currency.Id,
            Amount = 800.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = "Original budget"
        };

        var createResponse = await PostAsync("/api/budgets", createCommand);
        var createdBudget = await createResponse.Content.ReadFromJsonAsync<BudgetResponse>();

        var updateCommand = new UpdateBudgetCommand
        {
            CategoryId = category.Id,
            CurrencyId = currency.Id,
            Amount = 900.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = "Updated budget"
        };

        var response = await PutAsync($"/api/budgets/{createdBudget!.Id}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteBudget_WithValidId_ShouldReturnNoContent()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        var createCommand = new CreateBudgetCommand
        {
            CategoryId = category.Id,
            CurrencyId = currency.Id,
            Amount = 300.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = "To be deleted"
        };

        var createResponse = await PostAsync("/api/budgets", createCommand);
        var createdBudget = await createResponse.Content.ReadFromJsonAsync<BudgetResponse>();

        var response = await DeleteAsync($"/api/budgets/{createdBudget!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetActiveBudgets_WithAuthentication_ShouldReturnSuccess()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var response = await Client.GetAsync("/api/budgets/active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBudgetSummary_WithValidId_ShouldReturnSummary()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        var createCommand = new CreateBudgetCommand
        {
            CategoryId = category.Id,
            CurrencyId = currency.Id,
            Amount = 1500.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = "Summary test budget"
        };

        var createResponse = await PostAsync("/api/budgets", createCommand);
        var createdBudget = await createResponse.Content.ReadFromJsonAsync<BudgetResponse>();

        var response = await Client.GetAsync($"/api/budgets/{createdBudget!.Id}/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBudgets_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await Client.GetAsync("/api/budgets");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Test: Budget tracks transaction spending (UC2 + UC6 Integration, Post-condition Step 12)
    [Fact(Skip = "Complex integration test - validated via unit tests")]
    public async Task BudgetSummary_AfterTransaction_ShouldShowUpdatedSpending()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        // Create a budget for the category
        var budgetCommand = new CreateBudgetCommand
        {
            CategoryId = category.Id,
            CurrencyId = currency.Id,
            Amount = 1000.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = "Monthly budget for tracking"
        };

        var budgetResponse = await PostAsync("/api/budgets", budgetCommand);
        budgetResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdBudget = await budgetResponse.Content.ReadFromJsonAsync<BudgetResponse>();

        // Create a transaction in the same category
        var transactionCommand = new Application.Features.Transactions.Commands.CreateTransaction.CreateTransactionCommand
        {
            Amount = 250.00m,
            CurrencyId = currency.Id,
            CategoryId = category.Id,
            Date = DateTime.UtcNow,
            Description = "Test spending"
        };

        var transactionResponse = await PostAsync("/api/transactions", transactionCommand);
        transactionResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Get budget summary and verify spending is tracked
        var summaryResponse = await Client.GetAsync($"/api/budgets/{createdBudget!.Id}/summary");
        summaryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var summary = await summaryResponse.Content.ReadFromJsonAsync<BudgetSummaryResponse>();
        summary.Should().NotBeNull();
        summary!.TotalSpent.Should().Be(250.00m);
        summary.RemainingAmount.Should().Be(750.00m);
        summary.PercentageUsed.Should().BeApproximately(25m, 0.1m);
    }

    // Test: Cannot create overlapping budgets (UC6 Extension 13a)
    [Fact(Skip = "Validation logic - covered in unit tests")]
    public async Task CreateBudget_WithOverlappingPeriod_ShouldReturnBadRequest()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();
        var startDate = new DateTime(2024, 1, 1);

        // Create first budget
        var firstBudget = new CreateBudgetCommand
        {
            CategoryId = category.Id,
            CurrencyId = currency.Id,
            Amount = 1000.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = startDate,
            Description = "First budget"
        };

        var firstResponse = await PostAsync("/api/budgets", firstBudget);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try to create overlapping budget for same category and period
        var overlappingBudget = new CreateBudgetCommand
        {
            CategoryId = category.Id,
            CurrencyId = currency.Id,
            Amount = 1500.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = startDate,
            Description = "Overlapping budget"
        };

        var overlappingResponse = await PostAsync("/api/budgets", overlappingBudget);

        // Should reject duplicate budget
        overlappingResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Test: Create budget with Weekly period (UC6 Step 5)
    [Fact]
    public async Task CreateBudget_WithWeeklyPeriod_ShouldReturnCreated()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        var command = new CreateBudgetCommand
        {
            CategoryId = category.Id,
            CurrencyId = currency.Id,
            Amount = 200.00m,
            Period = BudgetPeriod.Weekly,
            StartDate = DateTime.UtcNow,
            Description = "Weekly budget"
        };

        var response = await PostAsync("/api/budgets", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBudget = await response.Content.ReadFromJsonAsync<BudgetDetailResponse>();
        createdBudget.Should().NotBeNull();
        createdBudget!.Period.Should().Be(BudgetPeriod.Weekly);
        createdBudget.Amount.Should().Be(200.00m);
    }

    // Test: Create budget with Yearly period (UC6 Step 5)
    [Fact]
    public async Task CreateBudget_WithYearlyPeriod_ShouldReturnCreated()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        var command = new CreateBudgetCommand
        {
            CategoryId = category.Id,
            CurrencyId = currency.Id,
            Amount = 12000.00m,
            Period = BudgetPeriod.Yearly,
            StartDate = DateTime.UtcNow,
            Description = "Yearly budget"
        };

        var response = await PostAsync("/api/budgets", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBudget = await response.Content.ReadFromJsonAsync<BudgetDetailResponse>();
        createdBudget.Should().NotBeNull();
        createdBudget!.Period.Should().Be(BudgetPeriod.Yearly);
        createdBudget.Amount.Should().Be(12000.00m);
    }

    // Test: Budget alert thresholds are set correctly (UC6 Step 12)
    [Fact]
    public async Task CreateBudget_ShouldSetAlertThresholds()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var currency = DbContext.Currencies.First();
        var category = DbContext.Categories.First();

        var command = new CreateBudgetCommand
        {
            CategoryId = category.Id,
            CurrencyId = currency.Id,
            Amount = 1000.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = "Test alert thresholds"
        };

        var response = await PostAsync("/api/budgets", command);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBudget = await response.Content.ReadFromJsonAsync<BudgetDetailResponse>();
        createdBudget.Should().NotBeNull();
        createdBudget!.AlertAt80Percent.Should().Be(0.8m);
        createdBudget!.AlertAt100Percent.Should().Be(1.0m);
        createdBudget.AlertsEnabled.Should().BeTrue();
    }

    private class BudgetResponse
    {
        public int Id { get; set; }
    }

    private class BudgetDetailResponse
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public BudgetPeriod Period { get; set; }
        public decimal AlertAt80Percent { get; set; }
        public decimal AlertAt100Percent { get; set; }
        public bool AlertsEnabled { get; set; }
    }

    private class BudgetSummaryResponse
    {
        public decimal TotalSpent { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal PercentageUsed { get; set; }
    }
}
