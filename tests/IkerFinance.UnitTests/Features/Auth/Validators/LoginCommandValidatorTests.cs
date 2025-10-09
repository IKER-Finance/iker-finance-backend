using FluentAssertions;
using FluentValidation.TestHelper;
using IkerFinance.Application.Features.Auth.Commands.Login;

namespace IkerFinance.UnitTests.Application.Features.Auth.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    // Test: Valid command passes validation
    [Fact]
    public void Validate_WithValidCommand_PassesValidation()
    {
        var command = new LoginCommand(
            Email: "test@example.com",
            Password: "Password123!"
        );

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Test: Empty email fails validation
    [Fact]
    public void Validate_WithEmptyEmail_FailsValidation()
    {
        var command = new LoginCommand(
            Email: "",
            Password: "Password123!"
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    // Test: Invalid email format fails validation
    [Fact]
    public void Validate_WithInvalidEmailFormat_FailsValidation()
    {
        var command = new LoginCommand(
            Email: "not-an-email",
            Password: "Password123!"
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Valid email address is required");
    }

    // Test: Empty password fails validation
    [Fact]
    public void Validate_WithEmptyPassword_FailsValidation()
    {
        var command = new LoginCommand(
            Email: "test@example.com",
            Password: ""
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    // Test: Multiple validation errors accumulate
    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        var command = new LoginCommand(
            Email: "",
            Password: ""
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    // Test: Validation doesn't enforce password strength (that's for Identity)
    [Fact]
    public void Validate_DoesNotEnforcePasswordStrength()
    {
        var command = new LoginCommand(
            Email: "test@example.com",
            Password: "weak"
        );

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}