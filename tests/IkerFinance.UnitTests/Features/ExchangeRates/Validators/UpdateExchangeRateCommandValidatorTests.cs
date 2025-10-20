using FluentAssertions;
using IkerFinance.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;

namespace IkerFinance.UnitTests.Features.ExchangeRates.Validators;

public class UpdateExchangeRateCommandValidatorTests
{
    private readonly UpdateExchangeRateCommandValidator _validator;

    public UpdateExchangeRateCommandValidatorTests()
    {
        _validator = new UpdateExchangeRateCommandValidator();
    }

    // Test: Valid command passes validation
    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        var command = new UpdateExchangeRateCommand
        {
            Id = 1,
            Rate = 1.35m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = "admin1",
            IsActive = true
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test: Id must be greater than zero
    [Fact]
    public void Validate_WithZeroId_ShouldHaveValidationError()
    {
        var command = new UpdateExchangeRateCommand
        {
            Id = 0,
            Rate = 1.35m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Id))
            .Which.ErrorMessage.Should().Be("Id must be greater than 0");
    }

    // Test: Id cannot be negative
    [Fact]
    public void Validate_WithNegativeId_ShouldHaveValidationError()
    {
        var command = new UpdateExchangeRateCommand
        {
            Id = -1,
            Rate = 1.35m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Id));
    }

    // Test: Rate must be greater than zero
    [Fact]
    public void Validate_WithZeroRate_ShouldHaveValidationError()
    {
        var command = new UpdateExchangeRateCommand
        {
            Id = 1,
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
        var command = new UpdateExchangeRateCommand
        {
            Id = 1,
            Rate = -1.35m,
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
        var command = new UpdateExchangeRateCommand
        {
            Id = 1,
            Rate = 1500000m,
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
        var command = new UpdateExchangeRateCommand
        {
            Id = 1,
            Rate = 1.35m,
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
        var command = new UpdateExchangeRateCommand
        {
            Id = 1,
            Rate = 1.35m,
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
        var command = new UpdateExchangeRateCommand
        {
            Id = 1,
            Rate = 1.35m,
            EffectiveDate = DateTime.UtcNow,
            AdminUserId = null!
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.AdminUserId));
    }
}
