using FluentAssertions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Budgets;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.Features.Budgets.Queries.GetBudgets;
using IkerFinance.Domain.Enums;
using Moq;

namespace IkerFinance.UnitTests.Application.Features.Budgets.Queries;

public class GetBudgetsQueryHandlerTests
{
    private readonly Mock<IBudgetRepository> _mockRepository;
    private readonly GetBudgetsQueryHandler _handler;
    private const string TestUserId = "user123";

    public GetBudgetsQueryHandlerTests()
    {
        _mockRepository = new Mock<IBudgetRepository>();
        _handler = new GetBudgetsQueryHandler(_mockRepository.Object);
    }

    // Test: Returns paginated budgets with valid query
    [Fact]
    public async Task Handle_WithValidQuery_ReturnsPaginatedBudgets()
    {
        var budgets = new List<BudgetDto>
        {
            new() { Id = 1, Amount = 1000m, CategoryName = "Food" },
            new() { Id = 2, Amount = 500m, CategoryName = "Transport" }
        };

        var paginatedResponse = new PaginatedResponse<BudgetDto>
        {
            Data = budgets,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mockRepository.Setup(x => x.GetBudgetsWithDetailsAsync(
            It.IsAny<BudgetFilters>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResponse);

        var query = new GetBudgetsQuery
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
        var paginatedResponse = new PaginatedResponse<BudgetDto>
        {
            Data = new List<BudgetDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        BudgetFilters? capturedFilters = null;
        _mockRepository.Setup(x => x.GetBudgetsWithDetailsAsync(
            It.IsAny<BudgetFilters>(),
            It.IsAny<CancellationToken>()))
            .Callback<BudgetFilters, CancellationToken>((filters, _) => capturedFilters = filters)
            .ReturnsAsync(paginatedResponse);

        var query = new GetBudgetsQuery
        {
            UserId = TestUserId,
            IsActive = true,
            Period = BudgetPeriod.Monthly,
            StartDate = startDate,
            EndDate = endDate,
            SearchTerm = "food",
            SortBy = "amount",
            SortOrder = "desc",
            PageNumber = 2,
            PageSize = 20
        };

        await _handler.Handle(query, CancellationToken.None);

        capturedFilters.Should().NotBeNull();
        capturedFilters!.UserId.Should().Be(TestUserId);
        capturedFilters.IsActive.Should().BeTrue();
        capturedFilters.Period.Should().Be(BudgetPeriod.Monthly);
        capturedFilters.StartDate.Should().Be(startDate);
        capturedFilters.EndDate.Should().Be(endDate);
        capturedFilters.SearchTerm.Should().Be("food");
        capturedFilters.SortBy.Should().Be("amount");
        capturedFilters.SortOrder.Should().Be("desc");
        capturedFilters.PageNumber.Should().Be(2);
        capturedFilters.PageSize.Should().Be(20);
    }

    // Test: Returns empty result when no budgets match filters
    [Fact]
    public async Task Handle_WhenNoBudgetsMatch_ReturnsEmptyResult()
    {
        var paginatedResponse = new PaginatedResponse<BudgetDto>
        {
            Data = new List<BudgetDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _mockRepository.Setup(x => x.GetBudgetsWithDetailsAsync(
            It.IsAny<BudgetFilters>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResponse);

        var query = new GetBudgetsQuery
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

    // Test: Filters by IsActive status
    [Fact]
    public async Task Handle_WithIsActiveFilter_FiltersCorrectly()
    {
        var paginatedResponse = new PaginatedResponse<BudgetDto>
        {
            Data = new List<BudgetDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        BudgetFilters? capturedFilters = null;
        _mockRepository.Setup(x => x.GetBudgetsWithDetailsAsync(
            It.IsAny<BudgetFilters>(),
            It.IsAny<CancellationToken>()))
            .Callback<BudgetFilters, CancellationToken>((filters, _) => capturedFilters = filters)
            .ReturnsAsync(paginatedResponse);

        var query = new GetBudgetsQuery
        {
            UserId = TestUserId,
            IsActive = false,
            PageNumber = 1,
            PageSize = 10
        };

        await _handler.Handle(query, CancellationToken.None);

        capturedFilters.Should().NotBeNull();
        capturedFilters!.IsActive.Should().BeFalse();
    }

    // Test: Filters by budget period
    [Fact]
    public async Task Handle_WithPeriodFilter_FiltersCorrectly()
    {
        var paginatedResponse = new PaginatedResponse<BudgetDto>
        {
            Data = new List<BudgetDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        BudgetFilters? capturedFilters = null;
        _mockRepository.Setup(x => x.GetBudgetsWithDetailsAsync(
            It.IsAny<BudgetFilters>(),
            It.IsAny<CancellationToken>()))
            .Callback<BudgetFilters, CancellationToken>((filters, _) => capturedFilters = filters)
            .ReturnsAsync(paginatedResponse);

        var query = new GetBudgetsQuery
        {
            UserId = TestUserId,
            Period = BudgetPeriod.Weekly,
            PageNumber = 1,
            PageSize = 10
        };

        await _handler.Handle(query, CancellationToken.None);

        capturedFilters.Should().NotBeNull();
        capturedFilters!.Period.Should().Be(BudgetPeriod.Weekly);
    }

    // Test: Handles search term normalization
    [Fact]
    public async Task Handle_NormalizesSearchTerm()
    {
        var paginatedResponse = new PaginatedResponse<BudgetDto>
        {
            Data = new List<BudgetDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        BudgetFilters? capturedFilters = null;
        _mockRepository.Setup(x => x.GetBudgetsWithDetailsAsync(
            It.IsAny<BudgetFilters>(),
            It.IsAny<CancellationToken>()))
            .Callback<BudgetFilters, CancellationToken>((filters, _) => capturedFilters = filters)
            .ReturnsAsync(paginatedResponse);

        var query = new GetBudgetsQuery
        {
            UserId = TestUserId,
            SearchTerm = "  Monthly Budget  ",
            PageNumber = 1,
            PageSize = 10
        };

        await _handler.Handle(query, CancellationToken.None);

        capturedFilters.Should().NotBeNull();
        capturedFilters!.SearchTerm.Should().Be("monthly budget");
    }

    // Test: Returns multiple pages correctly
    [Fact]
    public async Task Handle_WithMultiplePages_ReturnsCorrectPageInfo()
    {
        var paginatedResponse = new PaginatedResponse<BudgetDto>
        {
            Data = new List<BudgetDto>
            {
                new() { Id = 11, Amount = 1000m },
                new() { Id = 12, Amount = 2000m }
            },
            TotalCount = 25,
            PageNumber = 2,
            PageSize = 10
        };

        _mockRepository.Setup(x => x.GetBudgetsWithDetailsAsync(
            It.IsAny<BudgetFilters>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResponse);

        var query = new GetBudgetsQuery
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
        var paginatedResponse = new PaginatedResponse<BudgetDto>
        {
            Data = new List<BudgetDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        BudgetFilters? capturedFilters = null;
        _mockRepository.Setup(x => x.GetBudgetsWithDetailsAsync(
            It.IsAny<BudgetFilters>(),
            It.IsAny<CancellationToken>()))
            .Callback<BudgetFilters, CancellationToken>((filters, _) => capturedFilters = filters)
            .ReturnsAsync(paginatedResponse);

        var query = new GetBudgetsQuery
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
}
