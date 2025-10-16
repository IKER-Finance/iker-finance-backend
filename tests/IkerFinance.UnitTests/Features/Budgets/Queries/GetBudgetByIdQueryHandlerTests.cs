using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Budgets;
using IkerFinance.Application.Features.Budgets.Queries.GetBudgetById;
using IkerFinance.Domain.Enums;
using Moq;

namespace IkerFinance.UnitTests.Application.Features.Budgets.Queries;

public class GetBudgetByIdQueryHandlerTests
{
    private readonly Mock<IBudgetRepository> _mockRepository;
    private readonly GetBudgetByIdQueryHandler _handler;
    private const string TestUserId = "user123";

    public GetBudgetByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IBudgetRepository>();
        _handler = new GetBudgetByIdQueryHandler(_mockRepository.Object);
    }

    // Test: Returns budget when it exists and belongs to user
    [Fact]
    public async Task Handle_WithValidBudgetId_ReturnsBudget()
    {
        var budgetDto = new BudgetDto
        {
            Id = 1,
            CategoryId = 1,
            CategoryName = "Food",
            CategoryIcon = "utensils",
            CategoryColor = "#FF5733",
            Period = BudgetPeriod.Monthly,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 31, 23, 59, 59),
            Amount = 1000m,
            CurrencyId = 1,
            CurrencyCode = "USD",
            CurrencySymbol = "$",
            IsActive = true,
            Description = "Monthly food budget",
            AllowOverlap = false,
            AlertAt80Percent = 0.8m,
            AlertAt100Percent = 1.0m,
            AlertsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(x => x.GetBudgetWithDetailsAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budgetDto);

        var query = new GetBudgetByIdQuery { Id = 1, UserId = TestUserId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.CategoryName.Should().Be("Food");
        result.Amount.Should().Be(1000m);
        result.Period.Should().Be(BudgetPeriod.Monthly);
    }

    // Test: Throws NotFoundException when budget does not exist
    [Fact]
    public async Task Handle_WhenBudgetDoesNotExist_ThrowsNotFoundException()
    {
        _mockRepository.Setup(x => x.GetBudgetWithDetailsAsync(999, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BudgetDto?)null);

        var query = new GetBudgetByIdQuery { Id = 999, UserId = TestUserId };
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Budget*999*");
    }

    // Test: Throws NotFoundException when budget belongs to different user
    [Fact]
    public async Task Handle_WhenBudgetBelongsToDifferentUser_ThrowsNotFoundException()
    {
        _mockRepository.Setup(x => x.GetBudgetWithDetailsAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BudgetDto?)null);

        var query = new GetBudgetByIdQuery { Id = 1, UserId = TestUserId };
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Budget*1*");
    }

    // Test: Returns budget with all properties mapped correctly
    [Fact]
    public async Task Handle_ReturnsBudgetWithAllProperties()
    {
        var budgetDto = new BudgetDto
        {
            Id = 5,
            CategoryId = 3,
            CategoryName = "Entertainment",
            CategoryIcon = "film",
            CategoryColor = "#3498db",
            Period = BudgetPeriod.Weekly,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 7, 23, 59, 59),
            Amount = 200m,
            CurrencyId = 2,
            CurrencyCode = "EUR",
            CurrencySymbol = "€",
            IsActive = false,
            Description = "Weekly entertainment budget",
            AllowOverlap = true,
            AlertAt80Percent = 0.8m,
            AlertAt100Percent = 1.0m,
            AlertsEnabled = false,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _mockRepository.Setup(x => x.GetBudgetWithDetailsAsync(5, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budgetDto);

        var query = new GetBudgetByIdQuery { Id = 5, UserId = TestUserId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Id.Should().Be(5);
        result.CategoryId.Should().Be(3);
        result.CategoryName.Should().Be("Entertainment");
        result.CategoryIcon.Should().Be("film");
        result.CategoryColor.Should().Be("#3498db");
        result.Period.Should().Be(BudgetPeriod.Weekly);
        result.Amount.Should().Be(200m);
        result.CurrencyCode.Should().Be("EUR");
        result.CurrencySymbol.Should().Be("€");
        result.IsActive.Should().BeFalse();
        result.Description.Should().Be("Weekly entertainment budget");
        result.AllowOverlap.Should().BeTrue();
        result.AlertsEnabled.Should().BeFalse();
    }

    // Test: Calls repository with correct parameters
    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectParameters()
    {
        var budgetDto = new BudgetDto
        {
            Id = 10,
            Amount = 1000m,
            CategoryName = "Food",
            CurrencyCode = "USD"
        };

        _mockRepository.Setup(x => x.GetBudgetWithDetailsAsync(10, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budgetDto);

        var query = new GetBudgetByIdQuery { Id = 10, UserId = TestUserId };
        await _handler.Handle(query, CancellationToken.None);

        _mockRepository.Verify(
            x => x.GetBudgetWithDetailsAsync(10, TestUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
