using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Budgets.Queries.GetBudgetSummary;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.Budgets.Queries;

public class GetBudgetSummaryQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrencyConversionService> _mockConversionService;
    private readonly GetBudgetSummaryQueryHandler _handler;
    private const string TestUserId = "user123";

    public GetBudgetSummaryQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockConversionService = new Mock<ICurrencyConversionService>();
        _handler = new GetBudgetSummaryQueryHandler(
            _mockContext.Object,
            _mockConversionService.Object);
    }

    // Test: Returns budget summary with spending details successfully
    [Fact]
    public async Task Handle_WithValidBudget_ReturnsSummarySuccessfully()
    {
        var query = new GetBudgetSummaryQuery { UserId = TestUserId, BudgetId = 1 };

        var category = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };
        var startDate = DateTime.UtcNow.AddDays(-10);
        var endDate = DateTime.UtcNow.AddDays(20);

        var budget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Category = category,
            CurrencyId = 1,
            Currency = currency,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true,
            Description = "Monthly food budget"
        };

        var transactions = new[]
        {
            new Transaction
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                CurrencyId = 1,
                Currency = currency,
                ConvertedCurrencyId = 1,
                ConvertedCurrency = currency,
                Amount = 300m,
                ConvertedAmount = 300m,
                Date = startDate.AddDays(2),
                Type = TransactionType.Expense
            },
            new Transaction
            {
                Id = 2,
                UserId = TestUserId,
                CategoryId = 1,
                CurrencyId = 1,
                Currency = currency,
                ConvertedCurrencyId = 1,
                ConvertedCurrency = currency,
                Amount = 200m,
                ConvertedAmount = 200m,
                Date = startDate.AddDays(5),
                Type = TransactionType.Expense
            }
        };

        SetupMockDbSets(new[] { budget }, transactions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.BudgetId.Should().Be(1);
        result.CategoryName.Should().Be("Food");
        result.Amount.Should().Be(1000m);
        result.SpentAmount.Should().Be(500m);
        result.RemainingAmount.Should().Be(500m);
        result.PercentageSpent.Should().Be(50m);
        result.Status.Should().Be("OnTrack");
        result.TransactionCount.Should().Be(2);
        result.AlertAt80Percent.Should().BeFalse();
        result.AlertAt100Percent.Should().BeFalse();
    }

    // Test: Throws NotFoundException when budget does not exist
    [Fact]
    public async Task Handle_WhenBudgetDoesNotExist_ThrowsNotFoundException()
    {
        var query = new GetBudgetSummaryQuery { UserId = TestUserId, BudgetId = 999 };

        SetupMockDbSets(Array.Empty<Budget>(), Array.Empty<Transaction>());

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Budget*999*");
    }

    // Test: Throws NotFoundException when budget belongs to different user
    [Fact]
    public async Task Handle_WhenBudgetBelongsToDifferentUser_ThrowsNotFoundException()
    {
        var query = new GetBudgetSummaryQuery { UserId = TestUserId, BudgetId = 1 };

        var category = new Category { Id = 1, Name = "Food", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var budget = new Budget
        {
            Id = 1,
            UserId = "otherUser",
            CategoryId = 1,
            Category = category,
            CurrencyId = 1,
            Currency = currency,
            Amount = 1000m,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20)
        };

        SetupMockDbSets(new[] { budget }, Array.Empty<Transaction>());

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // Test: Correctly identifies Warning status when 80-99% spent
    [Fact]
    public async Task Handle_WithWarningStatus_SetsCorrectAlerts()
    {
        var query = new GetBudgetSummaryQuery { UserId = TestUserId, BudgetId = 1 };

        var category = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };
        var startDate = DateTime.UtcNow.AddDays(-10);
        var endDate = DateTime.UtcNow.AddDays(20);

        var budget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Category = category,
            CurrencyId = 1,
            Currency = currency,
            Amount = 1000m,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true
        };

        var transactions = new[]
        {
            new Transaction
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                CurrencyId = 1,
                Currency = currency,
                ConvertedCurrencyId = 1,
                ConvertedCurrency = currency,
                Amount = 850m,
                ConvertedAmount = 850m,
                Date = startDate.AddDays(2),
                Type = TransactionType.Expense
            }
        };

        SetupMockDbSets(new[] { budget }, transactions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be("Warning");
        result.PercentageSpent.Should().Be(85m);
        result.AlertAt80Percent.Should().BeTrue();
        result.AlertAt100Percent.Should().BeFalse();
    }

    // Test: Correctly identifies OverBudget status when 100%+ spent
    [Fact]
    public async Task Handle_WithOverBudgetStatus_SetsCorrectAlerts()
    {
        var query = new GetBudgetSummaryQuery { UserId = TestUserId, BudgetId = 1 };

        var category = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };
        var startDate = DateTime.UtcNow.AddDays(-10);
        var endDate = DateTime.UtcNow.AddDays(20);

        var budget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Category = category,
            CurrencyId = 1,
            Currency = currency,
            Amount = 1000m,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true
        };

        var transactions = new[]
        {
            new Transaction
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                CurrencyId = 1,
                Currency = currency,
                ConvertedCurrencyId = 1,
                ConvertedCurrency = currency,
                Amount = 1200m,
                ConvertedAmount = 1200m,
                Date = startDate.AddDays(2),
                Type = TransactionType.Expense
            }
        };

        SetupMockDbSets(new[] { budget }, transactions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be("OverBudget");
        result.PercentageSpent.Should().Be(120m);
        result.RemainingAmount.Should().Be(-200m);
        result.AlertAt80Percent.Should().BeFalse();
        result.AlertAt100Percent.Should().BeTrue();
    }

    // Test: Converts transactions to budget currency when currencies differ
    [Fact]
    public async Task Handle_WithDifferentCurrency_ConvertsAmountsCorrectly()
    {
        var query = new GetBudgetSummaryQuery { UserId = TestUserId, BudgetId = 1 };

        var category = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };
        var budgetCurrency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };
        var transactionCurrency = new Currency { Id = 2, Code = "EUR", Symbol = "€", IsActive = true };
        var startDate = DateTime.UtcNow.AddDays(-10);
        var endDate = DateTime.UtcNow.AddDays(20);

        var budget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Category = category,
            CurrencyId = 1,
            Currency = budgetCurrency,
            Amount = 1000m,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true
        };

        var transactions = new[]
        {
            new Transaction
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                CurrencyId = 2,
                Currency = transactionCurrency,
                ConvertedCurrencyId = 1,
                ConvertedCurrency = budgetCurrency,
                Amount = 100m,
                ConvertedAmount = 110m,
                Date = startDate.AddDays(2),
                Type = TransactionType.Expense
            }
        };

        SetupMockDbSets(new[] { budget }, transactions);

        _mockConversionService.Setup(x => x.ConvertAsync(110m, 1, 1))
            .ReturnsAsync(110m);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.SpentAmount.Should().Be(110m);
        result.RemainingAmount.Should().Be(890m);
        result.PercentageSpent.Should().Be(11m);
    }

    private void SetupMockDbSets(
        Budget[] budgets,
        Transaction[] transactions)
    {
        _mockContext.Setup(x => x.Budgets).Returns(budgets.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Transactions).Returns(transactions.AsQueryable().BuildMockDbSet().Object);
    }
}
