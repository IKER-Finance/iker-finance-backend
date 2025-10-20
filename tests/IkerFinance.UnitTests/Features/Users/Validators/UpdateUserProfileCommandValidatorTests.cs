using FluentAssertions;
using IkerFinance.Application.Features.Users.Commands.UpdateUserProfile;

namespace IkerFinance.UnitTests.Features.Users.Validators;

public class UpdateUserProfileCommandValidatorTests
{
    private readonly UpdateUserProfileCommandValidator _validator;

    public UpdateUserProfileCommandValidatorTests()
    {
        _validator = new UpdateUserProfileCommandValidator();
    }

    // Test: Valid command passes validation
    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        var command = new UpdateUserProfileCommand
        {
            UserId = "user1",
            FirstName = "John",
            LastName = "Doe"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test: FirstName is required
    [Fact]
    public void Validate_WithEmptyFirstName_ShouldHaveValidationError()
    {
        var command = new UpdateUserProfileCommand
        {
            UserId = "user1",
            FirstName = "",
            LastName = "Doe"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.FirstName))
            .Which.ErrorMessage.Should().Be("First name is required");
    }

    // Test: FirstName cannot be null
    [Fact]
    public void Validate_WithNullFirstName_ShouldHaveValidationError()
    {
        var command = new UpdateUserProfileCommand
        {
            UserId = "user1",
            FirstName = null!,
            LastName = "Doe"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.FirstName));
    }

    // Test: FirstName cannot exceed 100 characters
    [Fact]
    public void Validate_WithFirstNameExceedingMaxLength_ShouldHaveValidationError()
    {
        var command = new UpdateUserProfileCommand
        {
            UserId = "user1",
            FirstName = new string('a', 101),
            LastName = "Doe"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.FirstName))
            .Which.ErrorMessage.Should().Be("First name cannot exceed 100 characters");
    }

    // Test: FirstName at exactly 100 characters should pass validation
    [Fact]
    public void Validate_WithFirstNameAtMaxLength_ShouldNotHaveValidationError()
    {
        var command = new UpdateUserProfileCommand
        {
            UserId = "user1",
            FirstName = new string('a', 100),
            LastName = "Doe"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: LastName is required
    [Fact]
    public void Validate_WithEmptyLastName_ShouldHaveValidationError()
    {
        var command = new UpdateUserProfileCommand
        {
            UserId = "user1",
            FirstName = "John",
            LastName = ""
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.LastName))
            .Which.ErrorMessage.Should().Be("Last name is required");
    }

    // Test: LastName cannot be null
    [Fact]
    public void Validate_WithNullLastName_ShouldHaveValidationError()
    {
        var command = new UpdateUserProfileCommand
        {
            UserId = "user1",
            FirstName = "John",
            LastName = null!
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.LastName));
    }

    // Test: LastName cannot exceed 100 characters
    [Fact]
    public void Validate_WithLastNameExceedingMaxLength_ShouldHaveValidationError()
    {
        var command = new UpdateUserProfileCommand
        {
            UserId = "user1",
            FirstName = "John",
            LastName = new string('a', 101)
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.LastName))
            .Which.ErrorMessage.Should().Be("Last name cannot exceed 100 characters");
    }

    // Test: LastName at exactly 100 characters should pass validation
    [Fact]
    public void Validate_WithLastNameAtMaxLength_ShouldNotHaveValidationError()
    {
        var command = new UpdateUserProfileCommand
        {
            UserId = "user1",
            FirstName = "John",
            LastName = new string('a', 100)
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Both FirstName and LastName empty should produce multiple validation errors
    [Fact]
    public void Validate_WithBothNamesEmpty_ShouldHaveMultipleValidationErrors()
    {
        var command = new UpdateUserProfileCommand
        {
            UserId = "user1",
            FirstName = "",
            LastName = ""
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.FirstName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.LastName));
    }
}
