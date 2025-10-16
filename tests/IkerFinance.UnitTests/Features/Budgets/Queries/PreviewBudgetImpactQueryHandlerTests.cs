using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Budgets.Queries.PreviewBudgetImpact;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.Budgets.Queries;

public class PreviewBudgetImpactQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrencyConversionService> _mockConversionService;
    private readonly PreviewBudgetImpactQueryHandler _handler;
    private const string TestUserId = "user123";

    public PreviewBudgetImpactQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockConversionService = new Mock<ICurrencyConversionService>();
        _handler = new PreviewBudgetImpactQueryHandler(
            _mockContext.Object,
            _mockConversionService.Object);
    }

    // Test: Previews budget impact successfully for a transaction
    [Fact]
    public async Task Handle_WithValidTransaction_PreviewsImpactSuccessfully()
    {
        var transactionDate = DateTime.UtcNow;
        var query = new PreviewBudgetImpactQuery
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 300m,
            TransactionDate = transactionDate
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var budget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Category = category,
            CurrencyId = 1,
            Currency = currency,
            Amount = 1000m,
            StartDate = transactionDate.AddDays(-10),
            EndDate = transactionDate.AddDays(20),
            IsActive = true
        };

        var existingTransaction = new Transaction
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Currency = currency,
            Amount = 500m,
            ConvertedAmount = 500m,
            ConvertedCurrencyId = 1,
            Date = transactionDate.AddDays(-5),
            Type = TransactionType.Expense
        };

        SetupMockDbSets(new[] { user }, new[] { budget }, new[] { existingTransaction });

        _mockConversionService.Setup(x => x.ConvertAsync(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((decimal amount, int from, int to) => amount);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.AffectedBudgets.Should().HaveCount(1);

        var affectedBudget = result.AffectedBudgets[0];
        affectedBudget.BudgetId.Should().Be(1);
        affectedBudget.CategoryName.Should().Be("Food");
        affectedBudget.CurrentSpent.Should().Be(500m);
        affectedBudget.CurrentRemaining.Should().Be(500m);
        affectedBudget.CurrentPercentage.Should().Be(50m);
        affectedBudget.StatusBefore.Should().Be("OnTrack");
        affectedBudget.AfterSpent.Should().Be(800m);
        affectedBudget.AfterRemaining.Should().Be(200m);
        affectedBudget.AfterPercentage.Should().Be(80m);
        affectedBudget.StatusAfter.Should().Be("Warning");
        affectedBudget.WillTriggerAlert.Should().BeTrue();
    }

    // Test: Returns empty preview when no budgets are affected
    [Fact]
    public async Task Handle_WithNoBudgetsAffected_ReturnsEmptyPreview()
    {
        var query = new PreviewBudgetImpactQuery
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 100m,
            TransactionDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };

        SetupMockDbSets(new[] { user }, Array.Empty<Budget>(), Array.Empty<Transaction>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.AffectedBudgets.Should().BeEmpty();
        result.HasWarnings.Should().BeFalse();
        result.Warnings.Should().BeEmpty();
    }

    // Test: Triggers warning when transaction causes budget to exceed 100%
    [Fact]
    public async Task Handle_WhenTransactionCausesOverBudget_TriggersWarning()
    {
        var transactionDate = DateTime.UtcNow;
        var query = new PreviewBudgetImpactQuery
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 600m,
            TransactionDate = transactionDate
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var budget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Category = category,
            CurrencyId = 1,
            Currency = currency,
            Amount = 1000m,
            StartDate = transactionDate.AddDays(-10),
            EndDate = transactionDate.AddDays(20),
            IsActive = true
        };

        var existingTransaction = new Transaction
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Currency = currency,
            Amount = 500m,
            ConvertedAmount = 500m,
            ConvertedCurrencyId = 1,
            Date = transactionDate.AddDays(-5),
            Type = TransactionType.Expense
        };

        SetupMockDbSets(new[] { user }, new[] { budget }, new[] { existingTransaction });

        _mockConversionService.Setup(x => x.ConvertAsync(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((decimal amount, int from, int to) => amount);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.HasWarnings.Should().BeTrue();
        result.Warnings.Should().HaveCount(1);
        result.Warnings[0].Should().Contain("exceed its limit");

        var affectedBudget = result.AffectedBudgets[0];
        affectedBudget.StatusBefore.Should().Be("OnTrack");
        affectedBudget.StatusAfter.Should().Be("OverBudget");
        affectedBudget.AfterPercentage.Should().Be(110m);
        affectedBudget.WillTriggerAlert.Should().BeTrue();
    }

    // Test: Triggers alert when transaction pushes budget from OnTrack to Warning
    [Fact]
    public async Task Handle_WhenTransactionCausesWarning_TriggersAlert()
    {
        var transactionDate = DateTime.UtcNow;
        var query = new PreviewBudgetImpactQuery
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 400m,
            TransactionDate = transactionDate
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var budget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Category = category,
            CurrencyId = 1,
            Currency = currency,
            Amount = 1000m,
            StartDate = transactionDate.AddDays(-10),
            EndDate = transactionDate.AddDays(20),
            IsActive = true
        };

        var existingTransaction = new Transaction
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Currency = currency,
            Amount = 450m,
            ConvertedAmount = 450m,
            ConvertedCurrencyId = 1,
            Date = transactionDate.AddDays(-5),
            Type = TransactionType.Expense
        };

        SetupMockDbSets(new[] { user }, new[] { budget }, new[] { existingTransaction });

        _mockConversionService.Setup(x => x.ConvertAsync(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((decimal amount, int from, int to) => amount);

        var result = await _handler.Handle(query, CancellationToken.None);

        var affectedBudget = result.AffectedBudgets[0];
        affectedBudget.StatusBefore.Should().Be("OnTrack");
        affectedBudget.StatusAfter.Should().Be("Warning");
        affectedBudget.AfterPercentage.Should().Be(85m);
        affectedBudget.WillTriggerAlert.Should().BeTrue();
        affectedBudget.AlertMessage.Should().Contain("over 80% spent");
    }

    // Test: Converts transaction to budget currency when currencies differ
    [Fact]
    public async Task Handle_WithDifferentCurrency_ConvertsCorrectly()
    {
        var transactionDate = DateTime.UtcNow;
        var query = new PreviewBudgetImpactQuery
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 2,
            Amount = 100m,
            TransactionDate = transactionDate
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };
        var budgetCurrency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };
        var transactionCurrency = new Currency { Id = 2, Code = "EUR", Symbol = "€", IsActive = true };

        var budget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Category = category,
            CurrencyId = 1,
            Currency = budgetCurrency,
            Amount = 1000m,
            StartDate = transactionDate.AddDays(-10),
            EndDate = transactionDate.AddDays(20),
            IsActive = true
        };

        SetupMockDbSets(new[] { user }, new[] { budget }, Array.Empty<Transaction>());

        _mockConversionService.Setup(x => x.ConvertAsync(100m, 2, 1))
            .ReturnsAsync(110m);
        _mockConversionService.Setup(x => x.ConvertAsync(110m, 1, 1))
            .ReturnsAsync(110m);

        var result = await _handler.Handle(query, CancellationToken.None);

        var affectedBudget = result.AffectedBudgets[0];
        affectedBudget.AfterSpent.Should().Be(110m);
        affectedBudget.AfterPercentage.Should().Be(11m);
    }

    // Test: Throws BadRequestException when user has no home currency
    [Fact]
    public async Task Handle_WhenUserHasNoHomeCurrency_ThrowsBadRequestException()
    {
        var query = new PreviewBudgetImpactQuery
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 100m,
            TransactionDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = null };

        SetupMockDbSets(new[] { user }, Array.Empty<Budget>(), Array.Empty<Transaction>());

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    // Test: Only includes transactions within budget date range
    [Fact]
    public async Task Handle_OnlyIncludesTransactionsWithinBudgetDateRange()
    {
        var transactionDate = DateTime.UtcNow;
        var query = new PreviewBudgetImpactQuery
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 100m,
            TransactionDate = transactionDate
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var budget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Category = category,
            CurrencyId = 1,
            Currency = currency,
            Amount = 1000m,
            StartDate = transactionDate.AddDays(-10),
            EndDate = transactionDate.AddDays(20),
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
                Amount = 300m,
                ConvertedAmount = 300m,
                ConvertedCurrencyId = 1,
                Date = transactionDate.AddDays(-5),
                Type = TransactionType.Expense
            },
            new Transaction
            {
                Id = 2,
                UserId = TestUserId,
                CategoryId = 1,
                CurrencyId = 1,
                Currency = currency,
                Amount = 200m,
                ConvertedAmount = 200m,
                ConvertedCurrencyId = 1,
                Date = transactionDate.AddDays(-50),
                Type = TransactionType.Expense
            }
        };

        SetupMockDbSets(new[] { user }, new[] { budget }, transactions);

        _mockConversionService.Setup(x => x.ConvertAsync(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((decimal amount, int from, int to) => amount);

        var result = await _handler.Handle(query, CancellationToken.None);

        var affectedBudget = result.AffectedBudgets[0];
        affectedBudget.CurrentSpent.Should().Be(300m);
        affectedBudget.AfterSpent.Should().Be(400m);
    }

    private void SetupMockDbSets(
        ApplicationUser[] users,
        Budget[] budgets,
        Transaction[] transactions)
    {
        _mockContext.Setup(x => x.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Budgets).Returns(budgets.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Transactions).Returns(transactions.AsQueryable().BuildMockDbSet().Object);
    }
}
