using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Transactions;
using IkerFinance.Application.Features.Transactions.Queries.GetTransactionById;
using IkerFinance.Domain.Enums;
using Moq;

namespace IkerFinance.UnitTests.Application.Features.Transactions.Queries;

public class GetTransactionByIdQueryHandlerTests
{
    private readonly Mock<ITransactionRepository> _mockRepository;
    private readonly GetTransactionByIdQueryHandler _handler;
    private const string TestUserId = "user123";

    public GetTransactionByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<ITransactionRepository>();
        _handler = new GetTransactionByIdQueryHandler(_mockRepository.Object);
    }

    // Test: Returns transaction when it exists and belongs to user
    [Fact]
    public async Task Handle_WithValidTransactionId_ReturnsTransaction()
    {
        var transactionDto = new TransactionDto
        {
            Id = 1,
            Amount = 100m,
            CurrencyId = 1,
            CurrencyCode = "USD",
            CurrencySymbol = "$",
            ConvertedAmount = 100m,
            ConvertedCurrencyId = 1,
            ConvertedCurrencyCode = "USD",
            ExchangeRate = 1.0m,
            Type = TransactionType.Expense,
            Description = "Groceries",
            Notes = "Weekly shopping",
            Date = DateTime.UtcNow,
            CategoryId = 1,
            CategoryName = "Food",
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(x => x.GetTransactionWithDetailsAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionDto);

        var query = new GetTransactionByIdQuery { Id = 1, UserId = TestUserId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Amount.Should().Be(100m);
        result.Description.Should().Be("Groceries");
        result.Notes.Should().Be("Weekly shopping");
        result.CategoryName.Should().Be("Food");
    }

    // Test: Throws NotFoundException when transaction does not exist
    [Fact]
    public async Task Handle_WhenTransactionDoesNotExist_ThrowsNotFoundException()
    {
        _mockRepository.Setup(x => x.GetTransactionWithDetailsAsync(999, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransactionDto?)null);

        var query = new GetTransactionByIdQuery { Id = 999, UserId = TestUserId };
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Transaction*999*");
    }

    // Test: Throws NotFoundException when transaction belongs to different user
    [Fact]
    public async Task Handle_WhenTransactionBelongsToDifferentUser_ThrowsNotFoundException()
    {
        _mockRepository.Setup(x => x.GetTransactionWithDetailsAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransactionDto?)null);

        var query = new GetTransactionByIdQuery { Id = 1, UserId = TestUserId };
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Transaction*1*");
    }

    // Test: Returns transaction with all properties mapped correctly
    [Fact]
    public async Task Handle_ReturnsTransactionWithAllProperties()
    {
        var now = DateTime.UtcNow;
        var transactionDto = new TransactionDto
        {
            Id = 5,
            Amount = 250.50m,
            CurrencyId = 2,
            CurrencyCode = "EUR",
            CurrencySymbol = "€",
            ConvertedAmount = 275.55m,
            ConvertedCurrencyId = 1,
            ConvertedCurrencyCode = "USD",
            ExchangeRate = 1.1m,
            Type = TransactionType.Expense,
            Description = "Freelance payment",
            Notes = "Project completion",
            Date = now.AddDays(-2),
            CategoryId = 5,
            CategoryName = "Income",
            CreatedAt = now.AddDays(-2)
        };

        _mockRepository.Setup(x => x.GetTransactionWithDetailsAsync(5, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionDto);

        var query = new GetTransactionByIdQuery { Id = 5, UserId = TestUserId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Id.Should().Be(5);
        result.Amount.Should().Be(250.50m);
        result.CurrencyCode.Should().Be("EUR");
        result.CurrencySymbol.Should().Be("€");
        result.ConvertedAmount.Should().Be(275.55m);
        result.ConvertedCurrencyCode.Should().Be("USD");
        result.ExchangeRate.Should().Be(1.1m);
        result.Type.Should().Be(TransactionType.Expense);
        result.Description.Should().Be("Freelance payment");
        result.Notes.Should().Be("Project completion");
        result.CategoryName.Should().Be("Income");
    }

    // Test: Calls repository with correct parameters
    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectParameters()
    {
        var transactionDto = new TransactionDto
        {
            Id = 10,
            Amount = 100m,
            CurrencyCode = "USD",
            Description = "Test"
        };

        _mockRepository.Setup(x => x.GetTransactionWithDetailsAsync(10, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionDto);

        var query = new GetTransactionByIdQuery { Id = 10, UserId = TestUserId };
        await _handler.Handle(query, CancellationToken.None);

        _mockRepository.Verify(
            x => x.GetTransactionWithDetailsAsync(10, TestUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // Test: Returns transaction with exchange rate information
    [Fact]
    public async Task Handle_ReturnsTransactionWithExchangeRateInfo()
    {
        var transactionDto = new TransactionDto
        {
            Id = 3,
            Amount = 100m,
            CurrencyId = 2,
            CurrencyCode = "GBP",
            CurrencySymbol = "£",
            ConvertedAmount = 125m,
            ConvertedCurrencyId = 1,
            ConvertedCurrencyCode = "USD",
            ExchangeRate = 1.25m,
            Type = TransactionType.Expense,
            Description = "UK purchase",
            CategoryName = "Shopping"
        };

        _mockRepository.Setup(x => x.GetTransactionWithDetailsAsync(3, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionDto);

        var query = new GetTransactionByIdQuery { Id = 3, UserId = TestUserId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.CurrencyCode.Should().Be("GBP");
        result.ConvertedCurrencyCode.Should().Be("USD");
        result.ExchangeRate.Should().Be(1.25m);
        result.Amount.Should().Be(100m);
        result.ConvertedAmount.Should().Be(125m);
    }
}
