using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Application.Features.Feedbacks.Commands.UpdateFeedbackStatus;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;

namespace IkerFinance.UnitTests.Application.Features.Feedbacks.Commands;

public class UpdateFeedbackStatusCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly UpdateFeedbackStatusCommandHandler _handler;

    public UpdateFeedbackStatusCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new UpdateFeedbackStatusCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_WhenFeedbackNotFound_ShouldThrowNotFoundException()
    {
        var feedbacks = new List<Feedback>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Feedbacks).Returns(feedbacks.Object);

        var command = new UpdateFeedbackStatusCommand
        {
            Id = 123,
            AdminUserId = "admin1",
            Status = FeedbackStatus.Resolved
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Feedback*");
    }

    [Fact]
    public async Task Handle_WithValidFeedback_ShouldUpdateStatusSuccessfully()
    {
        var feedback = new Feedback { Id = 1, UserId = "u1", Status = FeedbackStatus.Open };
        var feedbacks = new List<Feedback> { feedback }.AsQueryable().BuildMockDbSet();

        var admin = new ApplicationUser { Id = "admin1", FirstName = "Jane", LastName = "Smith" };
        var users = new List<ApplicationUser> { admin }.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Feedbacks).Returns(feedbacks.Object);
        _mockContext.Setup(x => x.Users).Returns(users.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateFeedbackStatusCommand
        {
            Id = 1,
            AdminUserId = "admin1",
            Status = FeedbackStatus.Resolved,
            AdminResponse = "Fixed and verified."
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(FeedbackStatus.Resolved);
        result.AdminResponse.Should().Be("Fixed and verified.");
        result.RespondedByUserId.Should().Be("admin1");
        result.RespondedByUserName.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task Handle_WhenAdminUserNotFound_ShouldThrowNotFoundException()
    {
        var feedback = new Feedback { Id = 2, UserId = "u1", Status = FeedbackStatus.Open };
        var feedbacks = new List<Feedback> { feedback }.AsQueryable().BuildMockDbSet();
        var users = new List<ApplicationUser>().AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Feedbacks).Returns(feedbacks.Object);
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        var command = new UpdateFeedbackStatusCommand
        {
            Id = 2,
            AdminUserId = "missingAdmin",
            Status = FeedbackStatus.Resolved
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*User*");
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAndSaveOnce()
    {
        var feedback = new Feedback { Id = 10, UserId = "u1", Status = FeedbackStatus.Open };
        var feedbacks = new List<Feedback> { feedback }.AsQueryable().BuildMockDbSet();
        var admin = new ApplicationUser { Id = "admin1", FirstName = "Admin", LastName = "User" };
        var users = new List<ApplicationUser> { admin }.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Feedbacks).Returns(feedbacks.Object);
        _mockContext.Setup(x => x.Users).Returns(users.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateFeedbackStatusCommand
        {
            Id = 10,
            AdminUserId = "admin1",
            Status = FeedbackStatus.Resolved
        };

        await _handler.Handle(command, CancellationToken.None);

        _mockContext.Verify(x => x.Update(It.IsAny<Feedback>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
