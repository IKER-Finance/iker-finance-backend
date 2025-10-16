using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Transactions.Commands.CreateTransaction;
using IkerFinance.Domain.DomainServices.Transaction;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.Transactions.Commands;

public class CreateTransactionCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrencyConversionService> _mockConversionService;
    private readonly TransactionService _transactionService;
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockConversionService = new Mock<ICurrencyConversionService>();
        _transactionService = new TransactionService();
        _handler = new CreateTransactionCommandHandler(
            _mockContext.Object,
            _mockConversionService.Object,
            _transactionService);
    }

    // Test: Creates transaction successfully with valid data
    [Fact]
    public async Task Handle_WithValidData_CreatesTransactionSuccessfully()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Groceries",
            Notes = "Weekly shopping"
        };

        var user = new ApplicationUser { Id = "user123", HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, Array.Empty<ExchangeRate>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Amount.Should().Be(100m);
        result.CurrencyId.Should().Be(1);
        result.Description.Should().Be("Groceries");
        result.Notes.Should().Be("Weekly shopping");
        result.Type.Should().Be(TransactionType.Expense);
        _mockContext.Verify(x => x.Add(It.IsAny<Transaction>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test: Throws NotFoundException when user does not exist
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "nonexistent",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        SetupMockDbSets(Array.Empty<ApplicationUser>(), Array.Empty<Category>(), Array.Empty<Currency>(), Array.Empty<ExchangeRate>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*User*nonexistent*");
    }

    // Test: Throws NotFoundException when user has no home currency
    [Fact]
    public async Task Handle_WhenUserHasNoHomeCurrency_ThrowsNotFoundException()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        var user = new ApplicationUser { Id = "user123", HomeCurrencyId = null };

        SetupMockDbSets(new[] { user }, Array.Empty<Category>(), Array.Empty<Currency>(), Array.Empty<ExchangeRate>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // Test: Throws NotFoundException when category does not exist
    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ThrowsNotFoundException()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 999,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        var user = new ApplicationUser { Id = "user123", HomeCurrencyId = 1 };

        SetupMockDbSets(new[] { user }, Array.Empty<Category>(), Array.Empty<Currency>(), Array.Empty<ExchangeRate>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Category*999*");
    }

    // Test: Throws ValidationException when currency is inactive
    [Fact]
    public async Task Handle_WhenCurrencyIsInactive_ThrowsValidationException()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        var user = new ApplicationUser { Id = "user123", HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = false };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, Array.Empty<ExchangeRate>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // Test: Creates transaction with same currency as home currency without exchange rate
    [Fact]
    public async Task Handle_WithSameCurrencyAsHome_CreatesWithoutExchangeRate()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        var user = new ApplicationUser { Id = "user123", HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, Array.Empty<ExchangeRate>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ConvertedAmount.Should().Be(100m);
        result.ExchangeRate.Should().Be(1.0m);
        _mockConversionService.Verify(x => x.RatesExistAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    // Test: Creates transaction with different currency and applies exchange rate
    [Fact]
    public async Task Handle_WithDifferentCurrency_AppliesExchangeRate()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 2,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        var user = new ApplicationUser { Id = "user123", HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var transactionCurrency = new Currency { Id = 2, Code = "SEK", Symbol = "kr", IsActive = true };
        var homeCurrency = new Currency { Id = 1, Code = "BDT", Symbol = "৳", IsActive = true };
        var exchangeRate = new ExchangeRate { FromCurrencyId = 2, ToCurrencyId = 1, Rate = 10.5m, IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { transactionCurrency, homeCurrency }, new[] { exchangeRate });

        _mockConversionService.Setup(x => x.RatesExistAsync(2, 1)).ReturnsAsync(true);
        _mockConversionService.Setup(x => x.GetExchangeRateAsync(2, 1)).ReturnsAsync(exchangeRate);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Amount.Should().Be(100m);
        result.ConvertedAmount.Should().Be(1050m);
        result.ExchangeRate.Should().Be(10.5m);
        result.CurrencyCode.Should().Be("SEK");
        result.ConvertedCurrencyCode.Should().Be("BDT");
    }

    // Test: Throws ValidationException when exchange rate not available
    [Fact]
    public async Task Handle_WithDifferentCurrencyNoRate_ThrowsValidationException()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 2,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        var user = new ApplicationUser { Id = "user123", HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var currency = new Currency { Id = 2, Code = "EUR", Symbol = "€", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, Array.Empty<ExchangeRate>());

        _mockConversionService.Setup(x => x.RatesExistAsync(2, 1)).ReturnsAsync(false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // Test: Transaction inherits type from category
    [Fact]
    public async Task Handle_InheritsTypeFromCategory()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        var user = new ApplicationUser { Id = "user123", HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, Array.Empty<ExchangeRate>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Type.Should().Be(TransactionType.Expense);
        result.CategoryName.Should().Be("Food");
    }

    // Test: Sets CreatedAt timestamp
    [Fact]
    public async Task Handle_SetsCreatedAtTimestamp()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        var user = new ApplicationUser { Id = "user123", HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", Type = TransactionType.Expense };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, Array.Empty<ExchangeRate>());

        var beforeCreate = DateTime.UtcNow;
        var result = await _handler.Handle(command, CancellationToken.None);

        result.CreatedAt.Should().BeOnOrAfter(beforeCreate);
    }

    private void SetupMockDbSets(
        ApplicationUser[] users,
        Category[] categories,
        Currency[] currencies,
        ExchangeRate[] exchangeRates)
    {
        _mockContext.Setup(x => x.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Categories).Returns(categories.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Currencies).Returns(currencies.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.ExchangeRates).Returns(exchangeRates.AsQueryable().BuildMockDbSet().Object);
    }
}
