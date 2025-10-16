using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Budgets.Commands.DeleteBudget;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.Budgets.Commands;

public class DeleteBudgetCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly DeleteBudgetCommandHandler _handler;
    private const string TestUserId = "user123";

    public DeleteBudgetCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new DeleteBudgetCommandHandler(_mockContext.Object);
    }

    // Test: Deletes budget successfully when it exists and belongs to user
    [Fact]
    public async Task Handle_WithValidBudget_DeletesSuccessfully()
    {
        var budget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly
        };

        var budgets = new List<Budget> { budget };
        var mockDbSet = budgets.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Budgets).Returns(mockDbSet.Object);

        var command = new DeleteBudgetCommand { Id = 1, UserId = TestUserId };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        _mockContext.Verify(x => x.Remove(It.Is<Budget>(b => b.Id == 1)), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test: Throws NotFoundException when budget does not exist
    [Fact]
    public async Task Handle_WhenBudgetDoesNotExist_ThrowsNotFoundException()
    {
        var budgets = new List<Budget>();
        var mockDbSet = budgets.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Budgets).Returns(mockDbSet.Object);

        var command = new DeleteBudgetCommand { Id = 999, UserId = TestUserId };
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Budget*999*");
        _mockContext.Verify(x => x.Remove(It.IsAny<Budget>()), Times.Never);
    }

    // Test: Throws NotFoundException when budget belongs to different user
    [Fact]
    public async Task Handle_WhenBudgetBelongsToDifferentUser_ThrowsNotFoundException()
    {
        var budget = new Budget
        {
            Id = 1,
            UserId = "otherUser",
            CategoryId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly
        };

        var budgets = new List<Budget> { budget };
        var mockDbSet = budgets.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Budgets).Returns(mockDbSet.Object);

        var command = new DeleteBudgetCommand { Id = 1, UserId = TestUserId };
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Budget*1*");
        _mockContext.Verify(x => x.Remove(It.IsAny<Budget>()), Times.Never);
    }

    // Test: Correctly filters by both budget ID and user ID
    [Fact]
    public async Task Handle_FiltersBudgetByIdAndUserId()
    {
        var budget1 = new Budget { Id = 1, UserId = TestUserId, Amount = 1000m };
        var budget2 = new Budget { Id = 2, UserId = "otherUser", Amount = 2000m };
        var budget3 = new Budget { Id = 1, UserId = "anotherUser", Amount = 3000m };

        var budgets = new List<Budget> { budget1, budget2, budget3 };
        var mockDbSet = budgets.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Budgets).Returns(mockDbSet.Object);

        var command = new DeleteBudgetCommand { Id = 1, UserId = TestUserId };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        _mockContext.Verify(x => x.Remove(It.Is<Budget>(b => b.Id == 1 && b.UserId == TestUserId)), Times.Once);
    }

    // Test: Returns Unit.Value after successful deletion
    [Fact]
    public async Task Handle_AfterSuccessfulDeletion_ReturnsUnitValue()
    {
        var budget = new Budget { Id = 1, UserId = TestUserId, Amount = 1000m };
        var budgets = new List<Budget> { budget };
        var mockDbSet = budgets.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Budgets).Returns(mockDbSet.Object);

        var command = new DeleteBudgetCommand { Id = 1, UserId = TestUserId };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().Be(Unit.Value);
    }
}
