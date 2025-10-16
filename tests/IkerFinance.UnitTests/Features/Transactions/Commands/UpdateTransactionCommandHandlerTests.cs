using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Transactions.Commands.UpdateTransaction;
using IkerFinance.Domain.DomainServices.Transaction;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.Transactions.Commands;

public class UpdateTransactionCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrencyConversionService> _mockConversionService;
    private readonly TransactionService _transactionService;
    private readonly UpdateTransactionCommandHandler _handler;
    private const string TestUserId = "user123";

    public UpdateTransactionCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockConversionService = new Mock<ICurrencyConversionService>();
        _transactionService = new TransactionService();
        _handler = new UpdateTransactionCommandHandler(
            _mockContext.Object,
            _mockConversionService.Object,
            _transactionService);
    }

    // Test: Updates transaction successfully with valid data
    [Fact]
    public async Task Handle_WithValidData_UpdatesTransactionSuccessfully()
    {
        var existingTransaction = new Transaction
        {
            Id = 1,
            UserId = TestUserId,
            Amount = 50m,
            CurrencyId = 1,
            CategoryId = 1,
            Description = "Old Description"
        };

        var command = new UpdateTransactionCommand
        {
            Id = 1,
            UserId = TestUserId,
            Amount = 150m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Updated Groceries",
            Notes = "Updated notes"
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { existingTransaction }, new[] { user }, new[] { category }, new[] { currency }, Array.Empty<ExchangeRate>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Amount.Should().Be(150m);
        result.Description.Should().Be("Updated Groceries");
        result.Notes.Should().Be("Updated notes");
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test: Throws NotFoundException when transaction does not exist
    [Fact]
    public async Task Handle_WhenTransactionDoesNotExist_ThrowsNotFoundException()
    {
        var command = new UpdateTransactionCommand
        {
            Id = 999,
            UserId = TestUserId,
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        SetupMockDbSets(Array.Empty<Transaction>(), Array.Empty<ApplicationUser>(), Array.Empty<Category>(), Array.Empty<Currency>(), Array.Empty<ExchangeRate>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Transaction*999*");
    }

    // Test: Throws NotFoundException when transaction belongs to different user
    [Fact]
    public async Task Handle_WhenTransactionBelongsToDifferentUser_ThrowsNotFoundException()
    {
        var existingTransaction = new Transaction
        {
            Id = 1,
            UserId = "otherUser",
            Amount = 50m,
            CurrencyId = 1,
            CategoryId = 1
        };

        var command = new UpdateTransactionCommand
        {
            Id = 1,
            UserId = TestUserId,
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        SetupMockDbSets(new[] { existingTransaction }, Array.Empty<ApplicationUser>(), Array.Empty<Category>(), Array.Empty<Currency>(), Array.Empty<ExchangeRate>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Transaction*1*");
    }

    // Test: Throws ValidationException when user has no home currency
    [Fact]
    public async Task Handle_WhenUserHasNoHomeCurrency_ThrowsValidationException()
    {
        var existingTransaction = new Transaction { Id = 1, UserId = TestUserId, Amount = 50m };
        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = null };

        var command = new UpdateTransactionCommand
        {
            Id = 1,
            UserId = TestUserId,
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        SetupMockDbSets(new[] { existingTransaction }, new[] { user }, Array.Empty<Category>(), Array.Empty<Currency>(), Array.Empty<ExchangeRate>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // Test: Throws NotFoundException when category does not exist
    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ThrowsNotFoundException()
    {
        var existingTransaction = new Transaction { Id = 1, UserId = TestUserId, Amount = 50m };
        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };

        var command = new UpdateTransactionCommand
        {
            Id = 1,
            UserId = TestUserId,
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 999,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        SetupMockDbSets(new[] { existingTransaction }, new[] { user }, Array.Empty<Category>(), Array.Empty<Currency>(), Array.Empty<ExchangeRate>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Category*999*");
    }

    // Test: Throws ValidationException when currency is inactive
    [Fact]
    public async Task Handle_WhenCurrencyIsInactive_ThrowsValidationException()
    {
        var existingTransaction = new Transaction { Id = 1, UserId = TestUserId, Amount = 50m };
        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = false };

        var command = new UpdateTransactionCommand
        {
            Id = 1,
            UserId = TestUserId,
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        SetupMockDbSets(new[] { existingTransaction }, new[] { user }, new[] { category }, new[] { currency }, Array.Empty<ExchangeRate>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // Test: Updates transaction with same currency as home currency without exchange rate
    [Fact]
    public async Task Handle_WithSameCurrencyAsHome_UpdatesWithoutExchangeRate()
    {
        var existingTransaction = new Transaction { Id = 1, UserId = TestUserId, Amount = 50m, CurrencyId = 1, CategoryId = 1 };
        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var command = new UpdateTransactionCommand
        {
            Id = 1,
            UserId = TestUserId,
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        SetupMockDbSets(new[] { existingTransaction }, new[] { user }, new[] { category }, new[] { currency }, Array.Empty<ExchangeRate>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ConvertedAmount.Should().Be(100m);
        result.ExchangeRate.Should().Be(1.0m);
        _mockConversionService.Verify(x => x.RatesExistAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    // Test: Updates transaction with different currency and applies exchange rate
    [Fact]
    public async Task Handle_WithDifferentCurrency_AppliesExchangeRate()
    {
        var existingTransaction = new Transaction { Id = 1, UserId = TestUserId, Amount = 50m, CurrencyId = 1, CategoryId = 1 };
        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var transactionCurrency = new Currency { Id = 2, Code = "EUR", Symbol = "€", IsActive = true };
        var homeCurrency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };
        var exchangeRate = new ExchangeRate { FromCurrencyId = 2, ToCurrencyId = 1, Rate = 1.2m, IsActive = true };

        var command = new UpdateTransactionCommand
        {
            Id = 1,
            UserId = TestUserId,
            Amount = 100m,
            CurrencyId = 2,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        SetupMockDbSets(
            new[] { existingTransaction },
            new[] { user },
            new[] { category },
            new[] { transactionCurrency, homeCurrency },
            new[] { exchangeRate });

        _mockConversionService.Setup(x => x.RatesExistAsync(2, 1)).ReturnsAsync(true);
        _mockConversionService.Setup(x => x.GetExchangeRateAsync(2, 1)).ReturnsAsync(exchangeRate);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Amount.Should().Be(100m);
        result.ConvertedAmount.Should().Be(120m);
        result.ExchangeRate.Should().Be(1.2m);
        result.CurrencyCode.Should().Be("EUR");
        result.ConvertedCurrencyCode.Should().Be("USD");
    }

    // Test: Throws ValidationException when exchange rate not available
    [Fact]
    public async Task Handle_WithDifferentCurrencyNoRate_ThrowsValidationException()
    {
        var existingTransaction = new Transaction { Id = 1, UserId = TestUserId, Amount = 50m, CurrencyId = 1, CategoryId = 1 };
        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var currency = new Currency { Id = 2, Code = "EUR", Symbol = "€", IsActive = true };

        var command = new UpdateTransactionCommand
        {
            Id = 1,
            UserId = TestUserId,
            Amount = 100m,
            CurrencyId = 2,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        SetupMockDbSets(new[] { existingTransaction }, new[] { user }, new[] { category }, new[] { currency }, Array.Empty<ExchangeRate>());

        _mockConversionService.Setup(x => x.RatesExistAsync(2, 1)).ReturnsAsync(false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // Test: Maps all properties correctly to DTO
    [Fact]
    public async Task Handle_MapsPropertiesToDto()
    {
        var existingTransaction = new Transaction
        {
            Id = 1,
            UserId = TestUserId,
            Amount = 50m,
            CurrencyId = 1,
            CategoryId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var command = new UpdateTransactionCommand
        {
            Id = 1,
            UserId = TestUserId,
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Groceries",
            Notes = "Weekly shopping"
        };

        SetupMockDbSets(new[] { existingTransaction }, new[] { user }, new[] { category }, new[] { currency }, Array.Empty<ExchangeRate>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().Be(1);
        result.Amount.Should().Be(100m);
        result.CurrencyCode.Should().Be("USD");
        result.CurrencySymbol.Should().Be("$");
        result.CategoryName.Should().Be("Food");
        result.Type.Should().Be(TransactionType.Expense);
        result.Description.Should().Be("Groceries");
        result.Notes.Should().Be("Weekly shopping");
        result.CreatedAt.Should().Be(existingTransaction.CreatedAt);
    }

    private void SetupMockDbSets(
        Transaction[] transactions,
        ApplicationUser[] users,
        Category[] categories,
        Currency[] currencies,
        ExchangeRate[] exchangeRates)
    {
        _mockContext.Setup(x => x.Transactions).Returns(transactions.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Categories).Returns(categories.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Currencies).Returns(currencies.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.ExchangeRates).Returns(exchangeRates.AsQueryable().BuildMockDbSet().Object);
    }
}
