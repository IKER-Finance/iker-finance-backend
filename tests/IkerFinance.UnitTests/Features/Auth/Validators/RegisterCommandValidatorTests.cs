using FluentAssertions;
using FluentValidation.TestHelper;
using IkerFinance.Application.Features.Auth.Commands.Register;

namespace IkerFinance.UnitTests.Application.Features.Auth.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    // Test: Valid command passes validation
    [Fact]
    public void Validate_WithValidCommand_PassesValidation()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "John",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Test: Empty email fails validation
    [Fact]
    public void Validate_WithEmptyEmail_FailsValidation()
    {
        var command = new RegisterCommand(
            Email: "",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "John",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    // Test: Invalid email format fails validation
    [Fact]
    public void Validate_WithInvalidEmailFormat_FailsValidation()
    {
        var command = new RegisterCommand(
            Email: "not-an-email",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "John",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Valid email address is required");
    }

    // Test: Empty password fails validation
    [Fact]
    public void Validate_WithEmptyPassword_FailsValidation()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "",
            ConfirmPassword: "",
            FirstName: "John",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    // Test: Short password fails validation
    [Fact]
    public void Validate_WithShortPassword_FailsValidation()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Pass1",
            ConfirmPassword: "Pass1",
            FirstName: "John",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 6 characters");
    }

    // Test: Mismatched passwords fail validation
    [Fact]
    public void Validate_WithMismatchedPasswords_FailsValidation()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "DifferentPassword123!",
            FirstName: "John",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Passwords do not match");
    }

    // Test: Empty first name fails validation
    [Fact]
    public void Validate_WithEmptyFirstName_FailsValidation()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required");
    }

    // Test: Empty last name fails validation
    [Fact]
    public void Validate_WithEmptyLastName_FailsValidation()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "John",
            LastName: "",
            HomeCurrencyId: 1
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required");
    }

    // Test: First name exceeding max length fails validation
    [Fact]
    public void Validate_WithFirstNameTooLong_FailsValidation()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: new string('A', 51),
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name cannot exceed 50 characters");
    }

    // Test: Last name exceeding max length fails validation
    [Fact]
    public void Validate_WithLastNameTooLong_FailsValidation()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "John",
            LastName: new string('A', 51),
            HomeCurrencyId: 1
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name cannot exceed 50 characters");
    }

    // Test: Multiple validation errors accumulate
    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        var command = new RegisterCommand(
            Email: "",
            Password: "short",
            ConfirmPassword: "different",
            FirstName: "",
            LastName: "",
            HomeCurrencyId: 1
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
}