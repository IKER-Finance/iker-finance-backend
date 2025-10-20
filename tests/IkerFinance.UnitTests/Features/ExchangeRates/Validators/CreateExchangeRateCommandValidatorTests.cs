using FluentAssertions;
using IkerFinance.Application.Features.ExchangeRates.Commands.CreateExchangeRate;

namespace IkerFinance.UnitTests.Features.ExchangeRates.Validators;

public class CreateExchangeRateCommandValidatorTests
{
    private readonly CreateExchangeRateCommandValidator _validator;

    public CreateExchangeRateCommandValidatorTests()
    {
        _validator = new CreateExchangeRateCommandValidator();
    }

    // Test: Valid command passes validation
    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        var command = new CreateExchangeRateCommand
        {
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = 1.25m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = "admin1",
            IsActive = true
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test: FromCurrencyId must be greater than zero
    [Fact]
    public void Validate_WithZeroFromCurrencyId_ShouldHaveValidationError()
    {
        var command = new CreateExchangeRateCommand
        {
            FromCurrencyId = 0,
            ToCurrencyId = 2,
            Rate = 1.25m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.FromCurrencyId))
            .Which.ErrorMessage.Should().Be("FromCurrencyId must be greater than 0");
    }

    // Test: FromCurrencyId cannot be negative
    [Fact]
    public void Validate_WithNegativeFromCurrencyId_ShouldHaveValidationError()
    {
        var command = new CreateExchangeRateCommand
        {
            FromCurrencyId = -1,
            ToCurrencyId = 2,
            Rate = 1.25m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.FromCurrencyId));
    }

    // Test: ToCurrencyId must be greater than zero
    [Fact]
    public void Validate_WithZeroToCurrencyId_ShouldHaveValidationError()
    {
        var command = new CreateExchangeRateCommand
        {
            FromCurrencyId = 1,
            ToCurrencyId = 0,
            Rate = 1.25m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.ToCurrencyId))
            .Which.ErrorMessage.Should().Be("ToCurrencyId must be greater than 0");
    }

    // Test: FromCurrency and ToCurrency cannot be the same
    [Fact]
    public void Validate_WithSameFromAndToCurrency_ShouldHaveValidationError()
    {
        var command = new CreateExchangeRateCommand
        {
            FromCurrencyId = 1,
            ToCurrencyId = 1,
            Rate = 1.25m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "FromCurrencyId and ToCurrencyId cannot be the same");
    }

    // Test: Rate must be greater than zero
    [Fact]
    public void Validate_WithZeroRate_ShouldHaveValidationError()
    {
        var command = new CreateExchangeRateCommand
        {
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = 0,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Rate))
            .Which.ErrorMessage.Should().Be("Rate must be greater than 0");
    }

    // Test: Rate cannot be negative
    [Fact]
    public void Validate_WithNegativeRate_ShouldHaveValidationError()
    {
        var command = new CreateExchangeRateCommand
        {
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = -1.25m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Rate));
    }

    // Test: Rate cannot exceed maximum of 1,000,000
    [Fact]
    public void Validate_WithRateExceedingMaximum_ShouldHaveValidationError()
    {
        var command = new CreateExchangeRateCommand
        {
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = 1000001m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Rate))
            .Which.ErrorMessage.Should().Be("Rate cannot exceed 1,000,000");
    }

    // Test: EffectiveDate is required
    [Fact]
    public void Validate_WithEmptyEffectiveDate_ShouldHaveValidationError()
    {
        var command = new CreateExchangeRateCommand
        {
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = 1.25m,
            EffectiveDate = default,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.EffectiveDate));
    }

    // Test: AdminUserId is required
    [Fact]
    public void Validate_WithEmptyAdminUserId_ShouldHaveValidationError()
    {
        var command = new CreateExchangeRateCommand
        {
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = 1.25m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = ""
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.AdminUserId))
            .Which.ErrorMessage.Should().Be("AdminUserId is required");
    }

    // Test: AdminUserId cannot be null
    [Fact]
    public void Validate_WithNullAdminUserId_ShouldHaveValidationError()
    {
        var command = new CreateExchangeRateCommand
        {
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = 1.25m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = null!
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.AdminUserId));
    }
}
