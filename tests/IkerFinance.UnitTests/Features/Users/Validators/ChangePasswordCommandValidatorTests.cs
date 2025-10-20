using FluentAssertions;
using IkerFinance.Application.Features.Users.Commands.ChangePassword;

namespace IkerFinance.UnitTests.Features.Users.Validators;

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator;

    public ChangePasswordCommandValidatorTests()
    {
        _validator = new ChangePasswordCommandValidator();
    }

    // Test: Valid command passes validation
    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        var command = new ChangePasswordCommand
        {
            UserId = "user1",
            CurrentPassword = "OldPassword123",
            NewPassword = "NewPassword123"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test: Current password is required
    [Fact]
    public void Validate_WithEmptyCurrentPassword_ShouldHaveValidationError()
    {
        var command = new ChangePasswordCommand
        {
            UserId = "user1",
            CurrentPassword = "",
            NewPassword = "NewPassword123"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.CurrentPassword))
            .Which.ErrorMessage.Should().Be("Current password is required");
    }

    // Test: New password is required
    [Fact]
    public void Validate_WithEmptyNewPassword_ShouldHaveValidationError()
    {
        var command = new ChangePasswordCommand
        {
            UserId = "user1",
            CurrentPassword = "OldPassword123",
            NewPassword = ""
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.NewPassword) &&
            e.ErrorMessage == "New password is required");
    }

    // Test: Password must be at least 8 characters
    [Fact]
    public void Validate_WithNewPasswordLessThan8Characters_ShouldHaveValidationError()
    {
        var command = new ChangePasswordCommand
        {
            UserId = "user1",
            CurrentPassword = "OldPassword123",
            NewPassword = "Pass1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.NewPassword) &&
            e.ErrorMessage == "Password must be at least 8 characters");
    }

    // Test: Password must contain at least one uppercase letter
    [Fact]
    public void Validate_WithNewPasswordWithoutUppercase_ShouldHaveValidationError()
    {
        var command = new ChangePasswordCommand
        {
            UserId = "user1",
            CurrentPassword = "OldPassword123",
            NewPassword = "newpassword123"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.NewPassword) &&
            e.ErrorMessage == "Password must contain at least one uppercase letter");
    }

    // Test: Password must contain at least one lowercase letter
    [Fact]
    public void Validate_WithNewPasswordWithoutLowercase_ShouldHaveValidationError()
    {
        var command = new ChangePasswordCommand
        {
            UserId = "user1",
            CurrentPassword = "OldPassword123",
            NewPassword = "NEWPASSWORD123"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.NewPassword) &&
            e.ErrorMessage == "Password must contain at least one lowercase letter");
    }

    // Test: Password must contain at least one digit
    [Fact]
    public void Validate_WithNewPasswordWithoutDigit_ShouldHaveValidationError()
    {
        var command = new ChangePasswordCommand
        {
            UserId = "user1",
            CurrentPassword = "OldPassword123",
            NewPassword = "NewPassword"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.NewPassword) &&
            e.ErrorMessage == "Password must contain at least one digit");
    }

    // Test: Multiple valid password formats should pass validation
    [Theory]
    [InlineData("Password1")]
    [InlineData("MySecurePass123")]
    [InlineData("ComplexP@ssw0rd")]
    public void Validate_WithValidPasswords_ShouldNotHaveValidationErrors(string validPassword)
    {
        var command = new ChangePasswordCommand
        {
            UserId = "user1",
            CurrentPassword = "OldPassword123",
            NewPassword = validPassword
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
