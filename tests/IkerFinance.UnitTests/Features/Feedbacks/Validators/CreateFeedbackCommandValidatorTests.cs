using FluentAssertions;
using IkerFinance.Application.Features.Feedbacks.Commands.CreateFeedback;
using IkerFinance.Domain.Enums;

namespace IkerFinance.UnitTests.Features.Feedbacks.Validators;

public class CreateFeedbackCommandValidatorTests
{
    private readonly CreateFeedbackCommandValidator _validator;

    public CreateFeedbackCommandValidatorTests()
    {
        _validator = new CreateFeedbackCommandValidator();
    }

    // Test: Valid command passes validation
    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = "Test Feedback",
            Description = "This is a test feedback description",
            Type = FeedbackType.Bug,
            Priority = FeedbackPriority.High
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test: Subject is required
    [Fact]
    public void Validate_WithEmptySubject_ShouldHaveValidationError()
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = "",
            Description = "Test description",
            Type = FeedbackType.Bug,
            Priority = FeedbackPriority.Medium
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Subject))
            .Which.ErrorMessage.Should().Be("Subject is required");
    }

    // Test: Subject cannot be null
    [Fact]
    public void Validate_WithNullSubject_ShouldHaveValidationError()
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = null!,
            Description = "Test description",
            Type = FeedbackType.Feature,
            Priority = FeedbackPriority.Low
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Subject));
    }

    // Test: Subject cannot exceed 200 characters
    [Fact]
    public void Validate_WithSubjectExceedingMaxLength_ShouldHaveValidationError()
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = new string('a', 201),
            Description = "Test description",
            Type = FeedbackType.Improvement,
            Priority = FeedbackPriority.Medium
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Subject))
            .Which.ErrorMessage.Should().Be("Subject cannot exceed 200 characters");
    }

    // Test: Subject at maximum length is valid
    [Fact]
    public void Validate_WithSubjectAtMaxLength_ShouldNotHaveValidationError()
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = new string('a', 200),
            Description = "Test description",
            Type = FeedbackType.Question,
            Priority = FeedbackPriority.Low
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Description is required
    [Fact]
    public void Validate_WithEmptyDescription_ShouldHaveValidationError()
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = "Test Subject",
            Description = "",
            Type = FeedbackType.Bug,
            Priority = FeedbackPriority.High
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Description))
            .Which.ErrorMessage.Should().Be("Description is required");
    }

    // Test: Description cannot be null
    [Fact]
    public void Validate_WithNullDescription_ShouldHaveValidationError()
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = "Test Subject",
            Description = null!,
            Type = FeedbackType.Feature,
            Priority = FeedbackPriority.Critical
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Description));
    }

    // Test: Description cannot exceed 2000 characters
    [Fact]
    public void Validate_WithDescriptionExceedingMaxLength_ShouldHaveValidationError()
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = "Test Subject",
            Description = new string('a', 2001),
            Type = FeedbackType.Improvement,
            Priority = FeedbackPriority.Medium
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Description))
            .Which.ErrorMessage.Should().Be("Description cannot exceed 2000 characters");
    }

    // Test: Description at maximum length is valid
    [Fact]
    public void Validate_WithDescriptionAtMaxLength_ShouldNotHaveValidationError()
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = "Test Subject",
            Description = new string('a', 2000),
            Type = FeedbackType.Bug,
            Priority = FeedbackPriority.High
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Invalid feedback type should fail validation
    [Fact]
    public void Validate_WithInvalidFeedbackType_ShouldHaveValidationError()
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = "Test Subject",
            Description = "Test description",
            Type = (FeedbackType)999,
            Priority = FeedbackPriority.Medium
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Type) &&
            e.ErrorMessage == "Invalid feedback type");
    }

    // Test: Invalid priority should fail validation
    [Fact]
    public void Validate_WithInvalidPriority_ShouldHaveValidationError()
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = "Test Subject",
            Description = "Test description",
            Type = FeedbackType.Bug,
            Priority = (FeedbackPriority)999
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Priority) &&
            e.ErrorMessage == "Invalid priority");
    }

    // Test: All valid feedback types should pass validation
    [Theory]
    [InlineData(FeedbackType.Bug)]
    [InlineData(FeedbackType.Feature)]
    [InlineData(FeedbackType.Improvement)]
    [InlineData(FeedbackType.Question)]
    public void Validate_WithAllValidFeedbackTypes_ShouldNotHaveValidationError(FeedbackType type)
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = "Test Subject",
            Description = "Test description",
            Type = type,
            Priority = FeedbackPriority.Medium
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: All valid priorities should pass validation
    [Theory]
    [InlineData(FeedbackPriority.Low)]
    [InlineData(FeedbackPriority.Medium)]
    [InlineData(FeedbackPriority.High)]
    [InlineData(FeedbackPriority.Critical)]
    public void Validate_WithAllValidPriorities_ShouldNotHaveValidationError(FeedbackPriority priority)
    {
        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Subject = "Test Subject",
            Description = "Test description",
            Type = FeedbackType.Bug,
            Priority = priority
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
