using FluentAssertions;
using IkerFinance.Application.Features.Users.Commands.UpdateUserSettings;

namespace IkerFinance.UnitTests.Features.Users.Validators;

public class UpdateUserSettingsCommandValidatorTests
{
    private readonly UpdateUserSettingsCommandValidator _validator;

    public UpdateUserSettingsCommandValidatorTests()
    {
        _validator = new UpdateUserSettingsCommandValidator();
    }

    // Test: Valid command passes validation
    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        var command = new UpdateUserSettingsCommand
        {
            UserId = "user1",
            TimeZone = "UTC",
            DefaultTransactionCurrencyId = 1
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test: DefaultTransactionCurrencyId is optional
    [Fact]
    public void Validate_WithValidCommandWithoutDefaultCurrency_ShouldNotHaveValidationErrors()
    {
        var command = new UpdateUserSettingsCommand
        {
            UserId = "user1",
            TimeZone = "America/New_York",
            DefaultTransactionCurrencyId = null
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: TimeZone is required
    [Fact]
    public void Validate_WithEmptyTimeZone_ShouldHaveValidationError()
    {
        var command = new UpdateUserSettingsCommand
        {
            UserId = "user1",
            TimeZone = "",
            DefaultTransactionCurrencyId = 1
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.TimeZone))
            .Which.ErrorMessage.Should().Be("TimeZone is required");
    }

    // Test: TimeZone cannot be null
    [Fact]
    public void Validate_WithNullTimeZone_ShouldHaveValidationError()
    {
        var command = new UpdateUserSettingsCommand
        {
            UserId = "user1",
            TimeZone = null!,
            DefaultTransactionCurrencyId = 1
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.TimeZone));
    }

    // Test: TimeZone cannot exceed 50 characters
    [Fact]
    public void Validate_WithTimeZoneExceedingMaxLength_ShouldHaveValidationError()
    {
        var command = new UpdateUserSettingsCommand
        {
            UserId = "user1",
            TimeZone = new string('a', 51),
            DefaultTransactionCurrencyId = 1
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.TimeZone))
            .Which.ErrorMessage.Should().Be("TimeZone cannot exceed 50 characters");
    }

    // Test: TimeZone at exactly 50 characters should pass validation
    [Fact]
    public void Validate_WithTimeZoneAtMaxLength_ShouldNotHaveValidationError()
    {
        var command = new UpdateUserSettingsCommand
        {
            UserId = "user1",
            TimeZone = new string('a', 50),
            DefaultTransactionCurrencyId = 1
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: DefaultTransactionCurrencyId must be greater than zero when provided
    [Fact]
    public void Validate_WithZeroDefaultTransactionCurrencyId_ShouldHaveValidationError()
    {
        var command = new UpdateUserSettingsCommand
        {
            UserId = "user1",
            TimeZone = "UTC",
            DefaultTransactionCurrencyId = 0
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.DefaultTransactionCurrencyId))
            .Which.ErrorMessage.Should().Be("Default transaction currency ID must be greater than 0");
    }

    // Test: DefaultTransactionCurrencyId cannot be negative
    [Fact]
    public void Validate_WithNegativeDefaultTransactionCurrencyId_ShouldHaveValidationError()
    {
        var command = new UpdateUserSettingsCommand
        {
            UserId = "user1",
            TimeZone = "UTC",
            DefaultTransactionCurrencyId = -1
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.DefaultTransactionCurrencyId));
    }

    // Test: Multiple valid DefaultTransactionCurrencyId values should pass validation
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public void Validate_WithValidDefaultTransactionCurrencyIds_ShouldNotHaveValidationError(int currencyId)
    {
        var command = new UpdateUserSettingsCommand
        {
            UserId = "user1",
            TimeZone = "UTC",
            DefaultTransactionCurrencyId = currencyId
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Multiple valid TimeZone values should pass validation
    [Theory]
    [InlineData("UTC")]
    [InlineData("America/New_York")]
    [InlineData("Europe/London")]
    [InlineData("Asia/Tokyo")]
    public void Validate_WithValidTimeZones_ShouldNotHaveValidationErrors(string timeZone)
    {
        var command = new UpdateUserSettingsCommand
        {
            UserId = "user1",
            TimeZone = timeZone,
            DefaultTransactionCurrencyId = 1
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
