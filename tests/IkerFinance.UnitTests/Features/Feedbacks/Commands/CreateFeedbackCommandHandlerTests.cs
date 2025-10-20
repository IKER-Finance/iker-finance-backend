using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Application.Features.Feedbacks.Commands.CreateFeedback;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;

namespace IkerFinance.UnitTests.Application.Features.Feedbacks.Commands;

public class CreateFeedbackCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CreateFeedbackCommandHandler _handler;

    public CreateFeedbackCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new CreateFeedbackCommandHandler(_mockContext.Object);
    }

    // ✅ Test 1: 用户存在时成功创建反馈
    [Fact]
    public async Task Handle_WithValidUser_CreatesFeedbackSuccessfully()
    {
        var user = new ApplicationUser { Id = "user1", FirstName = "John", LastName = "Doe", Email = "john@doe.com" };
        var users = new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(users.Object);
        _mockContext.Setup(x => x.Add(It.IsAny<Feedback>()));
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateFeedbackCommand
        {
            UserId = "user1",
            Type = FeedbackType.Bug,
            Subject = "Crash on startup",
            Description = "App crashes immediately after login."
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.UserId.Should().Be("user1");
        result.UserName.Should().Be("John Doe");
        result.Subject.Should().Be(command.Subject);
        result.Description.Should().Be(command.Description);
        result.Status.Should().Be(FeedbackStatus.Open);
    }

    // ✅ Test 2: 用户不存在时应抛出 NotFoundException
    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        var emptyUsers = new List<ApplicationUser>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(emptyUsers.Object);

        var command = new CreateFeedbackCommand
        {
            UserId = "missingUser",
            Subject = "Test",
            Description = "User not found test"
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*User*");
    }

    // ✅ Test 3: 验证创建的 Feedback 属性是否正确
    [Fact]
    public async Task Handle_ShouldSetFeedbackPropertiesCorrectly()
    {
        var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "Smith" };
        var users = new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet();
        Feedback? capturedFeedback = null;

        _mockContext.Setup(x => x.Users).Returns(users.Object);
        _mockContext.Setup(x => x.Add(It.IsAny<Feedback>()))
            .Callback<object>(f => capturedFeedback = (Feedback)f);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateFeedbackCommand
        {
            UserId = "u1",
            Type = FeedbackType.Improvement,
            Subject = "Improve dashboard",
            Description = "Add more financial widgets"
        };

        await _handler.Handle(command, CancellationToken.None);

        capturedFeedback.Should().NotBeNull();
        capturedFeedback!.UserId.Should().Be("u1");
        capturedFeedback.Type.Should().Be(FeedbackType.Improvement);
        capturedFeedback.Status.Should().Be(FeedbackStatus.Open);
        capturedFeedback.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ✅ Test 4: 验证 Add() 和 SaveChangesAsync 被正确调用一次
    [Fact]
    public async Task Handle_ShouldCallAddAndSaveOnce()
    {
        var user = new ApplicationUser { Id = "u1", FirstName = "Jane", LastName = "Doe" };
        var users = new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(users.Object);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateFeedbackCommand
        {
            UserId = "u1",
            Type = FeedbackType.Question,
            Subject = "Feedback subject",
            Description = "Testing save operation"
        };

        await _handler.Handle(command, CancellationToken.None);

        _mockContext.Verify(x => x.Add(It.IsAny<Feedback>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
