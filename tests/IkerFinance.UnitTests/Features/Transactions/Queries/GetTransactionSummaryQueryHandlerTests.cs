using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Transactions.Queries.GetTransactionSummary;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.Transactions.Queries;

public class GetTransactionSummaryQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly GetTransactionSummaryQueryHandler _handler;
    private const string TestUserId = "user123";

    public GetTransactionSummaryQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new GetTransactionSummaryQueryHandler(_mockContext.Object);
    }

    // Test: Returns transaction summary with category breakdown successfully
    [Fact]
    public async Task Handle_WithTransactions_ReturnsSummarySuccessfully()
    {
        var query = new GetTransactionSummaryQuery
        {
            UserId = TestUserId,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var category1 = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", Type = TransactionType.Expense };
        var category2 = new Category { Id = 2, Name = "Transport", Icon = "🚗", Color = "#00FF00", Type = TransactionType.Expense };
        var category3 = new Category { Id = 3, Name = "Entertainment", Icon = "🎬", Color = "#0000FF", Type = TransactionType.Expense };

        var transactions = new[]
        {
            new Transaction
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                ConvertedAmount = 500m,
                ConvertedCurrencyId = 1,
                Date = DateTime.UtcNow.AddDays(-10),
                Type = TransactionType.Expense
            },
            new Transaction
            {
                Id = 2,
                UserId = TestUserId,
                CategoryId = 1,
                ConvertedAmount = 300m,
                ConvertedCurrencyId = 1,
                Date = DateTime.UtcNow.AddDays(-5),
                Type = TransactionType.Expense
            },
            new Transaction
            {
                Id = 3,
                UserId = TestUserId,
                CategoryId = 2,
                ConvertedAmount = 200m,
                ConvertedCurrencyId = 1,
                Date = DateTime.UtcNow.AddDays(-3),
                Type = TransactionType.Expense
            },
            new Transaction
            {
                Id = 4,
                UserId = TestUserId,
                CategoryId = 3,
                ConvertedAmount = 100m,
                ConvertedCurrencyId = 1,
                Date = DateTime.UtcNow.AddDays(-1),
                Type = TransactionType.Expense
            }
        };

        SetupMockDbSets(new[] { user }, new[] { currency }, new[] { category1, category2, category3 }, transactions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalExpenses.Should().Be(1100m);
        result.TotalTransactions.Should().Be(4);
        result.HomeCurrencyCode.Should().Be("USD");
        result.TopExpenseCategories.Should().HaveCount(3);
        result.TopExpenseCategories[0].CategoryName.Should().Be("Food");
        result.TopExpenseCategories[0].TotalAmount.Should().Be(800m);
        result.TopExpenseCategories[0].TransactionCount.Should().Be(2);
        result.TopExpenseCategories[0].Percentage.Should().BeApproximately(72.73m, 0.01m);
    }

    // Test: Throws NotFoundException when user does not exist
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        var query = new GetTransactionSummaryQuery { UserId = "nonexistent" };

        SetupMockDbSets(Array.Empty<ApplicationUser>(), Array.Empty<Currency>(), Array.Empty<Category>(), Array.Empty<Transaction>());

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // Test: Throws NotFoundException when user has no home currency
    [Fact]
    public async Task Handle_WhenUserHasNoHomeCurrency_ThrowsNotFoundException()
    {
        var query = new GetTransactionSummaryQuery { UserId = TestUserId };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = null };

        SetupMockDbSets(new[] { user }, Array.Empty<Currency>(), Array.Empty<Category>(), Array.Empty<Transaction>());

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // Test: Filters transactions by date range correctly
    [Fact]
    public async Task Handle_WithDateRange_FiltersTransactionsCorrectly()
    {
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        var query = new GetTransactionSummaryQuery
        {
            UserId = TestUserId,
            StartDate = startDate,
            EndDate = endDate
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };
        var category = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", Type = TransactionType.Expense };

        var transactions = new[]
        {
            new Transaction
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                ConvertedAmount = 100m,
                ConvertedCurrencyId = 1,
                Date = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc),
                Type = TransactionType.Expense
            },
            new Transaction
            {
                Id = 2,
                UserId = TestUserId,
                CategoryId = 1,
                ConvertedAmount = 200m,
                ConvertedCurrencyId = 1,
                Date = new DateTime(2024, 2, 15, 12, 0, 0, DateTimeKind.Utc),
                Type = TransactionType.Expense
            }
        };

        SetupMockDbSets(new[] { user }, new[] { currency }, new[] { category }, transactions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalTransactions.Should().Be(1);
        result.TotalExpenses.Should().Be(100m);
    }

    // Test: Returns top 5 categories by spending
    [Fact]
    public async Task Handle_ReturnsTop5CategoriesBySpending()
    {
        var query = new GetTransactionSummaryQuery { UserId = TestUserId };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var categories = Enumerable.Range(1, 7)
            .Select(i => new Category
            {
                Id = i,
                Name = $"Category{i}",
                Icon = "📦",
                Color = "#000000",
                Type = TransactionType.Expense
            })
            .ToArray();

        var transactions = Enumerable.Range(1, 7)
            .Select(i => new Transaction
            {
                Id = i,
                UserId = TestUserId,
                CategoryId = i,
                ConvertedAmount = i * 100m,
                ConvertedCurrencyId = 1,
                Date = DateTime.UtcNow.AddDays(-i),
                Type = TransactionType.Expense
            })
            .ToArray();

        SetupMockDbSets(new[] { user }, new[] { currency }, categories, transactions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TopExpenseCategories.Should().HaveCount(5);
        result.TopExpenseCategories[0].CategoryName.Should().Be("Category7");
        result.TopExpenseCategories[0].TotalAmount.Should().Be(700m);
        result.TopExpenseCategories[4].CategoryName.Should().Be("Category3");
        result.TopExpenseCategories[4].TotalAmount.Should().Be(300m);
    }

    // Test: Returns empty summary when no transactions exist
    [Fact]
    public async Task Handle_WithNoTransactions_ReturnsEmptySummary()
    {
        var query = new GetTransactionSummaryQuery { UserId = TestUserId };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { currency }, Array.Empty<Category>(), Array.Empty<Transaction>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalExpenses.Should().Be(0m);
        result.TotalTransactions.Should().Be(0);
        result.TopExpenseCategories.Should().BeEmpty();
    }

    // Test: Calculates category percentages correctly
    [Fact]
    public async Task Handle_CalculatesCategoryPercentagesCorrectly()
    {
        var query = new GetTransactionSummaryQuery { UserId = TestUserId };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var category1 = new Category { Id = 1, Name = "Food", Icon = "🍔", Color = "#FF0000", Type = TransactionType.Expense };
        var category2 = new Category { Id = 2, Name = "Transport", Icon = "🚗", Color = "#00FF00", Type = TransactionType.Expense };

        var transactions = new[]
        {
            new Transaction
            {
                Id = 1,
                UserId = TestUserId,
                CategoryId = 1,
                ConvertedAmount = 750m,
                ConvertedCurrencyId = 1,
                Date = DateTime.UtcNow,
                Type = TransactionType.Expense
            },
            new Transaction
            {
                Id = 2,
                UserId = TestUserId,
                CategoryId = 2,
                ConvertedAmount = 250m,
                ConvertedCurrencyId = 1,
                Date = DateTime.UtcNow,
                Type = TransactionType.Expense
            }
        };

        SetupMockDbSets(new[] { user }, new[] { currency }, new[] { category1, category2 }, transactions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TopExpenseCategories[0].Percentage.Should().Be(75m);
        result.TopExpenseCategories[1].Percentage.Should().Be(25m);
    }

    private void SetupMockDbSets(
        ApplicationUser[] users,
        Currency[] currencies,
        Category[] categories,
        Transaction[] transactions)
    {
        _mockContext.Setup(x => x.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Currencies).Returns(currencies.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Categories).Returns(categories.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Transactions).Returns(transactions.AsQueryable().BuildMockDbSet().Object);
    }
}
