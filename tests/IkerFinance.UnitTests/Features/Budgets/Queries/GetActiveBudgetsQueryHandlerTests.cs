using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Budgets.Queries.GetActiveBudgets;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.Budgets.Queries;

public class GetActiveBudgetsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrencyConversionService> _mockConversionService;
    private readonly GetActiveBudgetsQueryHandler _handler;
    private const string TestUserId = "user123";

    public GetActiveBudgetsQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockConversionService = new Mock<ICurrencyConversionService>();
        _handler = new GetActiveBudgetsQueryHandler(
            _mockContext.Object,
            _mockConversionService.Object);
    }

    // Test: Returns summary with all active budgets for user
    [Fact]
    public async Task Handle_WithActiveBudgets_ReturnsSummarySuccessfully()
    {
        var query = new GetActiveBudgetsQuery { UserId = TestUserId };

        var currentDate = DateTime.UtcNow;
        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };
        var category1 = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };
        var category2 = new Category { Id = 2, Name = "Transport", Icon = "🚗", Color = "#00FF00", IsActive = true };

        var budgets = new[]
        {
            new Budget
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                Category = category1,
                CurrencyId = 1,
                Currency = currency,
                Amount = 1000m,
                Period = BudgetPeriod.Monthly,
                StartDate = currentDate.AddDays(-10),
                EndDate = currentDate.AddDays(20),
                IsActive = true
            },
            new Budget
            {
                Id = 2,
                UserId = TestUserId,
                CategoryId = 2,
                Category = category2,
                CurrencyId = 1,
                Currency = currency,
                Amount = 500m,
                Period = BudgetPeriod.Monthly,
                StartDate = currentDate.AddDays(-10),
                EndDate = currentDate.AddDays(20),
                IsActive = true
            }
        };

        var transactions = new[]
        {
            new Transaction
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                Amount = 300m,
                ConvertedAmount = 300m,
                ConvertedCurrencyId = 1,
                Date = currentDate.AddDays(-5),
                Type = TransactionType.Expense
            },
            new Transaction
            {
                Id = 2,
                UserId = TestUserId,
                CategoryId = 2,
                Amount = 450m,
                ConvertedAmount = 450m,
                ConvertedCurrencyId = 1,
                Date = currentDate.AddDays(-3),
                Type = TransactionType.Expense
            }
        };

        SetupMockDbSets(new[] { user }, new[] { currency }, budgets, transactions);

        _mockConversionService.Setup(x => x.ConvertAsync(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((decimal amount, int from, int to) => amount);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalBudgets.Should().Be(2);
        result.TotalBudgetedAmount.Should().Be(1500m);
        result.TotalSpentAmount.Should().Be(750m);
        result.BudgetsOnTrack.Should().Be(1);
        result.BudgetsWarning.Should().Be(1);
        result.BudgetsOverBudget.Should().Be(0);
        result.HomeCurrencyCode.Should().Be("USD");
        result.Budgets.Should().HaveCount(2);
    }

    // Test: Returns empty summary when no active budgets exist
    [Fact]
    public async Task Handle_WithNoActiveBudgets_ReturnsEmptySummary()
    {
        var query = new GetActiveBudgetsQuery { UserId = TestUserId };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { currency }, Array.Empty<Budget>(), Array.Empty<Transaction>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalBudgets.Should().Be(0);
        result.TotalBudgetedAmount.Should().Be(0);
        result.TotalSpentAmount.Should().Be(0);
        result.BudgetsOnTrack.Should().Be(0);
        result.BudgetsWarning.Should().Be(0);
        result.BudgetsOverBudget.Should().Be(0);
        result.Budgets.Should().BeEmpty();
    }

    // Test: Only includes budgets within current date range
    [Fact]
    public async Task Handle_OnlyIncludesBudgetsWithinDateRange()
    {
        var query = new GetActiveBudgetsQuery { UserId = TestUserId };

        var currentDate = DateTime.UtcNow;
        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };
        var category = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };

        var budgets = new[]
        {
            new Budget
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                Category = category,
                CurrencyId = 1,
                Currency = currency,
                Amount = 1000m,
                StartDate = currentDate.AddDays(-10),
                EndDate = currentDate.AddDays(20),
                IsActive = true
            },
            new Budget
            {
                Id = 2,
                UserId = TestUserId,
                CategoryId = 1,
                Category = category,
                CurrencyId = 1,
                Currency = currency,
                Amount = 500m,
                StartDate = currentDate.AddDays(-100),
                EndDate = currentDate.AddDays(-50),
                IsActive = true
            }
        };

        SetupMockDbSets(new[] { user }, new[] { currency }, budgets, Array.Empty<Transaction>());

        _mockConversionService.Setup(x => x.ConvertAsync(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((decimal amount, int from, int to) => amount);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalBudgets.Should().Be(1);
        result.Budgets.Should().HaveCount(1);
        result.Budgets[0].Id.Should().Be(1);
    }

    // Test: Only includes active budgets (IsActive = true)
    [Fact]
    public async Task Handle_OnlyIncludesActiveBudgets()
    {
        var query = new GetActiveBudgetsQuery { UserId = TestUserId };

        var currentDate = DateTime.UtcNow;
        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };
        var category = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };

        var budgets = new[]
        {
            new Budget
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                Category = category,
                CurrencyId = 1,
                Currency = currency,
                Amount = 1000m,
                StartDate = currentDate.AddDays(-10),
                EndDate = currentDate.AddDays(20),
                IsActive = true
            },
            new Budget
            {
                Id = 2,
                UserId = TestUserId,
                CategoryId = 1,
                Category = category,
                CurrencyId = 1,
                Currency = currency,
                Amount = 500m,
                StartDate = currentDate.AddDays(-10),
                EndDate = currentDate.AddDays(20),
                IsActive = false
            }
        };

        SetupMockDbSets(new[] { user }, new[] { currency }, budgets, Array.Empty<Transaction>());

        _mockConversionService.Setup(x => x.ConvertAsync(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((decimal amount, int from, int to) => amount);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalBudgets.Should().Be(1);
        result.Budgets.Should().HaveCount(1);
        result.Budgets[0].Id.Should().Be(1);
    }

    // Test: Calculates budget status correctly (OnTrack, Warning, OverBudget)
    [Fact]
    public async Task Handle_CalculatesBudgetStatusCorrectly()
    {
        var query = new GetActiveBudgetsQuery { UserId = TestUserId };

        var currentDate = DateTime.UtcNow;
        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };
        var category1 = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };
        var category2 = new Category { Id = 2, Name = "Transport", Icon = "🚗", Color = "#00FF00", IsActive = true };
        var category3 = new Category { Id = 3, Name = "Entertainment", Icon = "🎬", Color = "#0000FF", IsActive = true };

        var budgets = new[]
        {
            new Budget
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                Category = category1,
                CurrencyId = 1,
                Currency = currency,
                Amount = 1000m,
                StartDate = currentDate.AddDays(-10),
                EndDate = currentDate.AddDays(20),
                IsActive = true
            },
            new Budget
            {
                Id = 2,
                UserId = TestUserId,
                CategoryId = 2,
                Category = category2,
                CurrencyId = 1,
                Currency = currency,
                Amount = 1000m,
                StartDate = currentDate.AddDays(-10),
                EndDate = currentDate.AddDays(20),
                IsActive = true
            },
            new Budget
            {
                Id = 3,
                UserId = TestUserId,
                CategoryId = 3,
                Category = category3,
                CurrencyId = 1,
                Currency = currency,
                Amount = 1000m,
                StartDate = currentDate.AddDays(-10),
                EndDate = currentDate.AddDays(20),
                IsActive = true
            }
        };

        var transactions = new[]
        {
            new Transaction
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                Amount = 500m,
                ConvertedAmount = 500m,
                ConvertedCurrencyId = 1,
                Date = currentDate.AddDays(-5),
                Type = TransactionType.Expense
            },
            new Transaction
            {
                Id = 2,
                UserId = TestUserId,
                CategoryId = 2,
                Amount = 850m,
                ConvertedAmount = 850m,
                ConvertedCurrencyId = 1,
                Date = currentDate.AddDays(-3),
                Type = TransactionType.Expense
            },
            new Transaction
            {
                Id = 3,
                UserId = TestUserId,
                CategoryId = 3,
                Amount = 1200m,
                ConvertedAmount = 1200m,
                ConvertedCurrencyId = 1,
                Date = currentDate.AddDays(-2),
                Type = TransactionType.Expense
            }
        };

        SetupMockDbSets(new[] { user }, new[] { currency }, budgets, transactions);

        _mockConversionService.Setup(x => x.ConvertAsync(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((decimal amount, int from, int to) => amount);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.BudgetsOnTrack.Should().Be(1);
        result.BudgetsWarning.Should().Be(1);
        result.BudgetsOverBudget.Should().Be(1);

        var onTrackBudget = result.Budgets.First(b => b.CategoryId == 1);
        onTrackBudget.Status.Should().Be("OnTrack");
        onTrackBudget.PercentageSpent.Should().Be(50m);

        var warningBudget = result.Budgets.First(b => b.CategoryId == 2);
        warningBudget.Status.Should().Be("Warning");
        warningBudget.PercentageSpent.Should().Be(85m);

        var overBudget = result.Budgets.First(b => b.CategoryId == 3);
        overBudget.Status.Should().Be("OverBudget");
        overBudget.PercentageSpent.Should().Be(120m);
    }

    // Test: Throws BadRequestException when user has no home currency
    [Fact]
    public async Task Handle_WhenUserHasNoHomeCurrency_ThrowsBadRequestException()
    {
        var query = new GetActiveBudgetsQuery { UserId = TestUserId };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = null };

        SetupMockDbSets(new[] { user }, Array.Empty<Currency>(), Array.Empty<Budget>(), Array.Empty<Transaction>());

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    private void SetupMockDbSets(
        ApplicationUser[] users,
        Currency[] currencies,
        Budget[] budgets,
        Transaction[] transactions)
    {
        _mockContext.Setup(x => x.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Currencies).Returns(currencies.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Budgets).Returns(budgets.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Transactions).Returns(transactions.AsQueryable().BuildMockDbSet().Object);
    }
}
