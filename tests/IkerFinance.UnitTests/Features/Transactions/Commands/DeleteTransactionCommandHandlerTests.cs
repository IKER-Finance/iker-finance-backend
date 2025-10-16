using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Transactions.Commands.DeleteTransaction;
using IkerFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.Transactions.Commands;

public class DeleteTransactionCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly DeleteTransactionCommandHandler _handler;
    private const string TestUserId = "user123";

    public DeleteTransactionCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new DeleteTransactionCommandHandler(_mockContext.Object);
    }

    // Test: Deletes transaction successfully when it exists and belongs to user
    [Fact]
    public async Task Handle_WithValidTransaction_DeletesSuccessfully()
    {
        var transaction = new Transaction
        {
            Id = 1,
            UserId = TestUserId,
            Amount = 100m,
            Description = "Test Transaction"
        };

        var transactions = new List<Transaction> { transaction };
        var mockDbSet = transactions.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Transactions).Returns(mockDbSet.Object);

        var command = new DeleteTransactionCommand { Id = 1, UserId = TestUserId };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        _mockContext.Verify(x => x.Remove(It.Is<Transaction>(t => t.Id == 1)), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test: Throws NotFoundException when transaction does not exist
    [Fact]
    public async Task Handle_WhenTransactionDoesNotExist_ThrowsNotFoundException()
    {
        var transactions = new List<Transaction>();
        var mockDbSet = transactions.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Transactions).Returns(mockDbSet.Object);

        var command = new DeleteTransactionCommand { Id = 999, UserId = TestUserId };
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Transaction*999*");
        _mockContext.Verify(x => x.Remove(It.IsAny<Transaction>()), Times.Never);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Test: Throws NotFoundException when transaction belongs to different user
    [Fact]
    public async Task Handle_WhenTransactionBelongsToDifferentUser_ThrowsNotFoundException()
    {
        var transaction = new Transaction
        {
            Id = 1,
            UserId = "otherUser",
            Amount = 100m,
            Description = "Test Transaction"
        };

        var transactions = new List<Transaction> { transaction };
        var mockDbSet = transactions.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Transactions).Returns(mockDbSet.Object);

        var command = new DeleteTransactionCommand { Id = 1, UserId = TestUserId };
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Transaction*1*");
        _mockContext.Verify(x => x.Remove(It.IsAny<Transaction>()), Times.Never);
    }

    // Test: Correctly filters by both transaction ID and user ID
    [Fact]
    public async Task Handle_FiltersTransactionByIdAndUserId()
    {
        var transaction1 = new Transaction { Id = 1, UserId = TestUserId, Amount = 100m };
        var transaction2 = new Transaction { Id = 2, UserId = "otherUser", Amount = 200m };
        var transaction3 = new Transaction { Id = 1, UserId = "anotherUser", Amount = 300m };

        var transactions = new List<Transaction> { transaction1, transaction2, transaction3 };
        var mockDbSet = transactions.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Transactions).Returns(mockDbSet.Object);

        var command = new DeleteTransactionCommand { Id = 1, UserId = TestUserId };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        _mockContext.Verify(x => x.Remove(It.Is<Transaction>(t => t.Id == 1 && t.UserId == TestUserId)), Times.Once);
    }

    // Test: Returns Unit.Value after successful deletion
    [Fact]
    public async Task Handle_AfterSuccessfulDeletion_ReturnsUnitValue()
    {
        var transaction = new Transaction { Id = 1, UserId = TestUserId, Amount = 100m };
        var transactions = new List<Transaction> { transaction };
        var mockDbSet = transactions.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Transactions).Returns(mockDbSet.Object);

        var command = new DeleteTransactionCommand { Id = 1, UserId = TestUserId };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().Be(Unit.Value);
    }
}
