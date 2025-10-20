using FluentAssertions;
using IkerFinance.Application.Features.Feedbacks.Commands.UpdateFeedbackStatus;
using IkerFinance.Domain.Enums;

namespace IkerFinance.UnitTests.Features.Feedbacks.Validators;

public class UpdateFeedbackStatusCommandValidatorTests
{
    private readonly UpdateFeedbackStatusCommandValidator _validator;

    public UpdateFeedbackStatusCommandValidatorTests()
    {
        _validator = new UpdateFeedbackStatusCommandValidator();
    }

    // Test: Valid command passes validation
    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        var command = new UpdateFeedbackStatusCommand
        {
            Id = 1,
            Status = FeedbackStatus.InProgress,
            AdminResponse = "We are working on this issue",
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test: AdminResponse is optional
    [Fact]
    public void Validate_WithValidCommandWithoutAdminResponse_ShouldNotHaveValidationErrors()
    {
        var command = new UpdateFeedbackStatusCommand
        {
            Id = 1,
            Status = FeedbackStatus.Open,
            AdminResponse = null,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Feedback ID is required
    [Fact]
    public void Validate_WithZeroId_ShouldHaveValidationError()
    {
        var command = new UpdateFeedbackStatusCommand
        {
            Id = 0,
            Status = FeedbackStatus.Responded,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Id))
            .Which.ErrorMessage.Should().Be("Feedback ID is required");
    }

    // Test: Id cannot be negative
    [Fact]
    public void Validate_WithNegativeId_ShouldHaveValidationError()
    {
        var command = new UpdateFeedbackStatusCommand
        {
            Id = -1,
            Status = FeedbackStatus.Resolved,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Id));
    }

    // Test: Invalid status should fail validation
    [Fact]
    public void Validate_WithInvalidStatus_ShouldHaveValidationError()
    {
        var command = new UpdateFeedbackStatusCommand
        {
            Id = 1,
            Status = (FeedbackStatus)999,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Status) &&
            e.ErrorMessage == "Invalid status");
    }

    // Test: AdminResponse cannot exceed 2000 characters
    [Fact]
    public void Validate_WithAdminResponseExceedingMaxLength_ShouldHaveValidationError()
    {
        var command = new UpdateFeedbackStatusCommand
        {
            Id = 1,
            Status = FeedbackStatus.Responded,
            AdminResponse = new string('a', 2001),
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.AdminResponse))
            .Which.ErrorMessage.Should().Be("Admin response cannot exceed 2000 characters");
    }

    // Test: AdminResponse at maximum length is valid
    [Fact]
    public void Validate_WithAdminResponseAtMaxLength_ShouldNotHaveValidationError()
    {
        var command = new UpdateFeedbackStatusCommand
        {
            Id = 1,
            Status = FeedbackStatus.Responded,
            AdminResponse = new string('a', 2000),
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Empty AdminResponse is valid
    [Fact]
    public void Validate_WithEmptyAdminResponse_ShouldNotHaveValidationError()
    {
        var command = new UpdateFeedbackStatusCommand
        {
            Id = 1,
            Status = FeedbackStatus.InProgress,
            AdminResponse = "",
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Whitespace AdminResponse bypasses length validation
    [Fact]
    public void Validate_WithWhitespaceAdminResponse_ShouldNotValidateLength()
    {
        // Arrange - whitespace should bypass the length validation per the "When" clause
        var command = new UpdateFeedbackStatusCommand
        {
            Id = 1,
            Status = FeedbackStatus.Open,
            AdminResponse = "   ",
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: All valid statuses should pass validation
    [Theory]
    [InlineData(FeedbackStatus.Open)]
    [InlineData(FeedbackStatus.InProgress)]
    [InlineData(FeedbackStatus.Responded)]
    [InlineData(FeedbackStatus.Resolved)]
    [InlineData(FeedbackStatus.Closed)]
    public void Validate_WithAllValidStatuses_ShouldNotHaveValidationError(FeedbackStatus status)
    {
        var command = new UpdateFeedbackStatusCommand
        {
            Id = 1,
            Status = status,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Multiple invalid Id values should fail validation
    [Theory]
    [InlineData(-100)]
    [InlineData(-1)]
    [InlineData(0)]
    public void Validate_WithInvalidIds_ShouldHaveValidationError(int invalidId)
    {
        var command = new UpdateFeedbackStatusCommand
        {
            Id = invalidId,
            Status = FeedbackStatus.Open,
            AdminUserId = "admin1"
        };

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
        var command = new UpdateFeedbackStatusCommand
        {
            Id = validId,
            Status = FeedbackStatus.Resolved,
            AdminUserId = "admin1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
