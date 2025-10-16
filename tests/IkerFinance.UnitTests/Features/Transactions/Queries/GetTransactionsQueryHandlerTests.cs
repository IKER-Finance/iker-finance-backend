using FluentAssertions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.Transactions;
using IkerFinance.Application.Features.Transactions.Queries.GetTransactions;
using IkerFinance.Domain.Enums;
using Moq;

namespace IkerFinance.UnitTests.Application.Features.Transactions.Queries;

public class GetTransactionsQueryHandlerTests
{
    private readonly Mock<ITransactionRepository> _mockRepository;
    private readonly GetTransactionsQueryHandler _handler;
    private const string TestUserId = "user123";

    public GetTransactionsQueryHandlerTests()
    {
        _mockRepository = new Mock<ITransactionRepository>();
        _handler = new GetTransactionsQueryHandler(_mockRepository.Object);
    }

    // Test: Returns paginated transactions with valid query
    [Fact]
    public async Task Handle_WithValidQuery_ReturnsPaginatedTransactions()
    {
        var transactions = new List<TransactionDto>
        {
            new() { Id = 1, Amount = 100m, Description = "Transaction 1" },
            new() { Id = 2, Amount = 200m, Description = "Transaction 2" }
        };

        var paginatedResponse = new PaginatedResponse<TransactionDto>
        {
            Data = transactions,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mockRepository.Setup(x => x.GetTransactionsWithDetailsAsync(
            It.IsAny<TransactionFilters>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResponse);

        var query = new GetTransactionsQuery
        {
            UserId = TestUserId,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    // Test: Passes correct filters to repository
    [Fact]
    public async Task Handle_PassesCorrectFiltersToRepository()
    {
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var paginatedResponse = new PaginatedResponse<TransactionDto>
        {
            Data = new List<TransactionDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        TransactionFilters? capturedFilters = null;
        _mockRepository.Setup(x => x.GetTransactionsWithDetailsAsync(
            It.IsAny<TransactionFilters>(),
            It.IsAny<CancellationToken>()))
            .Callback<TransactionFilters, CancellationToken>((filters, _) => capturedFilters = filters)
            .ReturnsAsync(paginatedResponse);

        var query = new GetTransactionsQuery
        {
            UserId = TestUserId,
            StartDate = startDate,
            EndDate = endDate,
            CategoryId = 1,
            CurrencyId = 2,
            Type = (int)TransactionType.Expense,
            SearchTerm = "groceries",
            SortBy = "date",
            SortOrder = "desc",
            PageNumber = 2,
            PageSize = 20
        };

        await _handler.Handle(query, CancellationToken.None);

        capturedFilters.Should().NotBeNull();
        capturedFilters!.UserId.Should().Be(TestUserId);
        capturedFilters.StartDate.Should().Be(startDate);
        capturedFilters.EndDate.Should().Be(endDate);
        capturedFilters.CategoryId.Should().Be(1);
        capturedFilters.CurrencyId.Should().Be(2);
        capturedFilters.Type.Should().Be(TransactionType.Expense);
        capturedFilters.SearchTerm.Should().Be("groceries");
        capturedFilters.SortBy.Should().Be("date");
        capturedFilters.SortOrder.Should().Be("desc");
        capturedFilters.PageNumber.Should().Be(2);
        capturedFilters.PageSize.Should().Be(20);
    }

    // Test: Returns empty result when no transactions match filters
    [Fact]
    public async Task Handle_WhenNoTransactionsMatch_ReturnsEmptyResult()
    {
        var paginatedResponse = new PaginatedResponse<TransactionDto>
        {
            Data = new List<TransactionDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _mockRepository.Setup(x => x.GetTransactionsWithDetailsAsync(
            It.IsAny<TransactionFilters>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResponse);

        var query = new GetTransactionsQuery
        {
            UserId = TestUserId,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    // Test: Filters by transaction type correctly
    [Fact]
    public async Task Handle_WithTypeFilter_FiltersCorrectly()
    {
        var paginatedResponse = new PaginatedResponse<TransactionDto>
        {
            Data = new List<TransactionDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        TransactionFilters? capturedFilters = null;
        _mockRepository.Setup(x => x.GetTransactionsWithDetailsAsync(
            It.IsAny<TransactionFilters>(),
            It.IsAny<CancellationToken>()))
            .Callback<TransactionFilters, CancellationToken>((filters, _) => capturedFilters = filters)
            .ReturnsAsync(paginatedResponse);

        var query = new GetTransactionsQuery
        {
            UserId = TestUserId,
            Type = (int)TransactionType.Expense,
            PageNumber = 1,
            PageSize = 10
        };

        await _handler.Handle(query, CancellationToken.None);

        capturedFilters.Should().NotBeNull();
        capturedFilters!.Type.Should().Be(TransactionType.Expense);
    }

    // Test: Handles search term normalization
    [Fact]
    public async Task Handle_NormalizesSearchTerm()
    {
        var paginatedResponse = new PaginatedResponse<TransactionDto>
        {
            Data = new List<TransactionDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        TransactionFilters? capturedFilters = null;
        _mockRepository.Setup(x => x.GetTransactionsWithDetailsAsync(
            It.IsAny<TransactionFilters>(),
            It.IsAny<CancellationToken>()))
            .Callback<TransactionFilters, CancellationToken>((filters, _) => capturedFilters = filters)
            .ReturnsAsync(paginatedResponse);

        var query = new GetTransactionsQuery
        {
            UserId = TestUserId,
            SearchTerm = "  Test Search  ",
            PageNumber = 1,
            PageSize = 10
        };

        await _handler.Handle(query, CancellationToken.None);

        capturedFilters.Should().NotBeNull();
        capturedFilters!.SearchTerm.Should().Be("test search");
    }

    // Test: Returns multiple pages correctly
    [Fact]
    public async Task Handle_WithMultiplePages_ReturnsCorrectPageInfo()
    {
        var paginatedResponse = new PaginatedResponse<TransactionDto>
        {
            Data = new List<TransactionDto>
            {
                new() { Id = 11, Amount = 100m },
                new() { Id = 12, Amount = 200m }
            },
            TotalCount = 25,
            PageNumber = 2,
            PageSize = 10
        };

        _mockRepository.Setup(x => x.GetTransactionsWithDetailsAsync(
            It.IsAny<TransactionFilters>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResponse);

        var query = new GetTransactionsQuery
        {
            UserId = TestUserId,
            PageNumber = 2,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.Data.Should().HaveCount(2);
    }

    // Test: Filters by date range
    [Fact]
    public async Task Handle_WithDateRange_FiltersCorrectly()
    {
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var paginatedResponse = new PaginatedResponse<TransactionDto>
        {
            Data = new List<TransactionDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        TransactionFilters? capturedFilters = null;
        _mockRepository.Setup(x => x.GetTransactionsWithDetailsAsync(
            It.IsAny<TransactionFilters>(),
            It.IsAny<CancellationToken>()))
            .Callback<TransactionFilters, CancellationToken>((filters, _) => capturedFilters = filters)
            .ReturnsAsync(paginatedResponse);

        var query = new GetTransactionsQuery
        {
            UserId = TestUserId,
            StartDate = startDate,
            EndDate = endDate,
            PageNumber = 1,
            PageSize = 10
        };

        await _handler.Handle(query, CancellationToken.None);

        capturedFilters.Should().NotBeNull();
        capturedFilters!.StartDate.Should().Be(startDate);
        capturedFilters.EndDate.Should().Be(endDate);
    }

    // Test: Filters by category and currency
    [Fact]
    public async Task Handle_WithCategoryAndCurrency_FiltersCorrectly()
    {
        var paginatedResponse = new PaginatedResponse<TransactionDto>
        {
            Data = new List<TransactionDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        TransactionFilters? capturedFilters = null;
        _mockRepository.Setup(x => x.GetTransactionsWithDetailsAsync(
            It.IsAny<TransactionFilters>(),
            It.IsAny<CancellationToken>()))
            .Callback<TransactionFilters, CancellationToken>((filters, _) => capturedFilters = filters)
            .ReturnsAsync(paginatedResponse);

        var query = new GetTransactionsQuery
        {
            UserId = TestUserId,
            CategoryId = 5,
            CurrencyId = 3,
            PageNumber = 1,
            PageSize = 10
        };

        await _handler.Handle(query, CancellationToken.None);

        capturedFilters.Should().NotBeNull();
        capturedFilters!.CategoryId.Should().Be(5);
        capturedFilters.CurrencyId.Should().Be(3);
    }
}
