using FluentAssertions;
using IkerFinance.Application.Features.ExchangeRates.Commands.DeleteExchangeRate;

namespace IkerFinance.UnitTests.Features.ExchangeRates.Validators;

public class DeleteExchangeRateCommandValidatorTests
{
    private readonly DeleteExchangeRateCommandValidator _validator;

    public DeleteExchangeRateCommandValidatorTests()
    {
        _validator = new DeleteExchangeRateCommandValidator();
    }

    // Test: Valid command passes validation
    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        var command = new DeleteExchangeRateCommand { Id = 1 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test: Id must be greater than zero
    [Fact]
    public void Validate_WithZeroId_ShouldHaveValidationError()
    {
        var command = new DeleteExchangeRateCommand { Id = 0 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Id))
            .Which.ErrorMessage.Should().Be("Id must be greater than 0");
    }

    // Test: Id cannot be negative
    [Fact]
    public void Validate_WithNegativeId_ShouldHaveValidationError()
    {
        var command = new DeleteExchangeRateCommand { Id = -1 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Id));
    }

    // Test: Multiple invalid Id values should fail validation
    [Theory]
    [InlineData(-100)]
    [InlineData(-1)]
    [InlineData(0)]
    public void Validate_WithInvalidIds_ShouldHaveValidationError(int invalidId)
    {
        var command = new DeleteExchangeRateCommand { Id = invalidId };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Id));
    }

    // Test: Multiple valid Id values should pass validation
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public void Validate_WithValidIds_ShouldNotHaveValidationErrors(int validId)
    {
        var command = new DeleteExchangeRateCommand { Id = validId };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
