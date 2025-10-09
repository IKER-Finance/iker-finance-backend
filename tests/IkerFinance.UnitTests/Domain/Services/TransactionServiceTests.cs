using FluentAssertions;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using IkerFinance.Domain.Services;

namespace IkerFinance.UnitTests.Domain.Services;

public class TransactionServiceTests
{
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _service = new TransactionService();
    }

    // Test: When transaction currency equals home currency, exchange rate should be 1.0
    [Fact]
    public void Create_WithSameCurrency_SetsExchangeRateToOne()
    {
        var userId = "user123";
        var currencyId = 1;
        var homeCurrencyId = 1;
        var amount = 100m;

        var result = _service.Create(
            userId: userId,
            amount: amount,
            currencyId: currencyId,
            homeCurrencyId: homeCurrencyId,
            categoryId: 5,
            type: TransactionType.Expense,
            description: "Test",
            notes: null,
            date: DateTime.UtcNow,
            exchangeRate: null
        );

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Amount.Should().Be(amount);
        result.ConvertedAmount.Should().Be(amount);
        result.ExchangeRate.Should().Be(1.0m);
        result.CurrencyId.Should().Be(currencyId);
        result.ConvertedCurrencyId.Should().Be(homeCurrencyId);
    }

    // Test: Cross-currency conversion applies exchange rate correctly
    [Fact]
    public void Create_WithCrossCurrency_CalculatesConversionCorrectly()
    {
        var exchangeRate = new ExchangeRate
        {
            FromCurrencyId = 2,
            ToCurrencyId = 1,
            Rate = 15.0m,
            EffectiveDate = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        };

        var result = _service.Create(
            userId: "user123",
            amount: 100m,
            currencyId: 2,
            homeCurrencyId: 1,
            categoryId: 5,
            type: TransactionType.Expense,
            description: "Test",
            notes: null,
            date: DateTime.UtcNow,
            exchangeRate: exchangeRate
        );

        result.Amount.Should().Be(100m);
        result.ConvertedAmount.Should().Be(1500m);
        result.ExchangeRate.Should().Be(15.0m);
    }

    // Test: Cross-currency transaction without exchange rate throws exception
    [Fact]
    public void Create_WithCrossCurrencyAndNullRate_ThrowsException()
    {
        var act = () => _service.Create(
            userId: "user123",
            amount: 100m,
            currencyId: 2,
            homeCurrencyId: 1,
            categoryId: 5,
            type: TransactionType.Expense,
            description: "Test",
            notes: null,
            date: DateTime.UtcNow,
            exchangeRate: null
        );

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Exchange rate is required*");
    }

    // Test: Inactive exchange rate is rejected
    [Fact]
    public void Create_WithInactiveExchangeRate_ThrowsException()
    {
        var exchangeRate = new ExchangeRate
        {
            FromCurrencyId = 2,
            ToCurrencyId = 1,
            Rate = 15.0m,
            EffectiveDate = DateTime.UtcNow.AddDays(-1),
            IsActive = false
        };

        var act = () => _service.Create(
            userId: "user123",
            amount: 100m,
            currencyId: 2,
            homeCurrencyId: 1,
            categoryId: 5,
            type: TransactionType.Expense,
            description: "Test",
            notes: null,
            date: DateTime.UtcNow,
            exchangeRate: exchangeRate
        );

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not currently valid*");
    }

    // Test: Exchange rate with future effective date is rejected
    [Fact]
    public void Create_WithFutureEffectiveDate_ThrowsException()
    {
        var exchangeRate = new ExchangeRate
        {
            FromCurrencyId = 2,
            ToCurrencyId = 1,
            Rate = 15.0m,
            EffectiveDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        };

        var act = () => _service.Create(
            userId: "user123",
            amount: 100m,
            currencyId: 2,
            homeCurrencyId: 1,
            categoryId: 5,
            type: TransactionType.Expense,
            description: "Test",
            notes: null,
            date: DateTime.UtcNow,
            exchangeRate: exchangeRate
        );

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not currently valid*");
    }

    // Test: Updating transaction to same currency sets exchange rate to 1.0
    [Fact]
    public void Update_ChangingToSameCurrency_SetsExchangeRateToOne()
    {
        var transaction = new Transaction
        {
            Id = 1,
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 2,
            ConvertedAmount = 1500m,
            ConvertedCurrencyId = 1,
            ExchangeRate = 15.0m
        };

        _service.Update(
            transaction: transaction,
            amount: 200m,
            currencyId: 1,
            homeCurrencyId: 1,
            categoryId: 5,
            type: TransactionType.Expense,
            description: "Updated",
            notes: "New notes",
            date: DateTime.UtcNow,
            exchangeRate: null
        );

        transaction.Amount.Should().Be(200m);
        transaction.ConvertedAmount.Should().Be(200m);
        transaction.ExchangeRate.Should().Be(1.0m);
        transaction.CurrencyId.Should().Be(1);
        transaction.Description.Should().Be("Updated");
        transaction.Notes.Should().Be("New notes");
    }

    // Test: Updating transaction to cross-currency calculates conversion correctly
    [Fact]
    public void Update_ChangingToCrossCurrency_CalculatesConversionCorrectly()
    {
        var transaction = new Transaction
        {
            Id = 1,
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            ConvertedAmount = 100m,
            ConvertedCurrencyId = 1,
            ExchangeRate = 1.0m
        };

        var exchangeRate = new ExchangeRate
        {
            FromCurrencyId = 2,
            ToCurrencyId = 1,
            Rate = 15.0m,
            EffectiveDate = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        };

        _service.Update(
            transaction: transaction,
            amount: 50m,
            currencyId: 2,
            homeCurrencyId: 1,
            categoryId: 5,
            type: TransactionType.Income,
            description: "Updated",
            notes: null,
            date: DateTime.UtcNow,
            exchangeRate: exchangeRate
        );

        transaction.Amount.Should().Be(50m);
        transaction.ConvertedAmount.Should().Be(750m);
        transaction.ExchangeRate.Should().Be(15.0m);
        transaction.CurrencyId.Should().Be(2);
        transaction.Type.Should().Be(TransactionType.Income);
    }

    // Test: Updating to cross-currency without exchange rate throws exception
    [Fact]
    public void Update_WithCrossCurrencyAndNullRate_ThrowsException()
    {
        var transaction = new Transaction
        {
            Id = 1,
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1
        };

        var act = () => _service.Update(
            transaction: transaction,
            amount: 100m,
            currencyId: 2,
            homeCurrencyId: 1,
            categoryId: 5,
            type: TransactionType.Expense,
            description: "Test",
            notes: null,
            date: DateTime.UtcNow,
            exchangeRate: null
        );

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Exchange rate is required*");
    }
}