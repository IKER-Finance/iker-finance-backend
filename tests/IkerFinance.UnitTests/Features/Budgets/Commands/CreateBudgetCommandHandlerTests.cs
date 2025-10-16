using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Budgets.Commands.CreateBudget;
using IkerFinance.Domain.DomainServices.Budget;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.Budgets.Commands;

public class CreateBudgetCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrencyConversionService> _mockConversionService;
    private readonly BudgetService _budgetService;
    private readonly CreateBudgetCommandHandler _handler;
    private const string TestUserId = "user123";

    public CreateBudgetCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockConversionService = new Mock<ICurrencyConversionService>();
        _budgetService = new BudgetService();
        _handler = new CreateBudgetCommandHandler(
            _mockContext.Object,
            _mockConversionService.Object,
            _budgetService);
    }

    // Test: Creates budget successfully with valid data
    [Fact]
    public async Task Handle_WithValidData_CreatesBudgetSuccessfully()
    {
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = "Monthly food budget"
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, Array.Empty<Budget>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Amount.Should().Be(1000m);
        result.CategoryName.Should().Be("Food");
        result.Period.Should().Be(BudgetPeriod.Monthly);
        _mockContext.Verify(x => x.Add(It.IsAny<Budget>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    // Test: Throws NotFoundException when user does not exist
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "nonexistent",
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        SetupMockDbSets(Array.Empty<ApplicationUser>(), Array.Empty<Category>(), Array.Empty<Currency>(), Array.Empty<Budget>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*User*");
    }

    // Test: Throws NotFoundException when user has no home currency
    [Fact]
    public async Task Handle_WhenUserHasNoHomeCurrency_ThrowsNotFoundException()
    {
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = null };

        SetupMockDbSets(new[] { user }, Array.Empty<Category>(), Array.Empty<Currency>(), Array.Empty<Budget>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // Test: Throws ValidationException when currency is inactive
    [Fact]
    public async Task Handle_WhenCurrencyIsInactive_ThrowsValidationException()
    {
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = false };

        SetupMockDbSets(new[] { user }, Array.Empty<Category>(), new[] { currency }, Array.Empty<Budget>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // Test: Throws NotFoundException when category does not exist
    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ThrowsNotFoundException()
    {
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 999,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { user }, Array.Empty<Category>(), new[] { currency }, Array.Empty<Budget>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Category*999*");
    }

    // Test: Throws ValidationException when exchange rate not available for different currency
    [Fact]
    public async Task Handle_WithDifferentCurrencyNoRate_ThrowsValidationException()
    {
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 2,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", IsActive = true };
        var currency = new Currency { Id = 2, Code = "EUR", Symbol = "€", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, Array.Empty<Budget>());

        _mockConversionService.Setup(x => x.RatesExistAsync(2, 1)).ReturnsAsync(false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // Test: Creates budget with different currency when exchange rate exists
    [Fact]
    public async Task Handle_WithDifferentCurrencyAndRate_CreatesBudgetSuccessfully()
    {
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 2,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", IsActive = true };
        var currency = new Currency { Id = 2, Code = "EUR", Symbol = "€", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, Array.Empty<Budget>());

        _mockConversionService.Setup(x => x.RatesExistAsync(2, 1)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.CurrencyCode.Should().Be("EUR");
    }

    // Test: Throws ValidationException when overlapping budget exists
    [Fact]
    public async Task Handle_WhenOverlappingBudgetExists_ThrowsValidationException()
    {
        var startDate = new DateTime(2024, 1, 1);
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = startDate
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var existingBudget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Period = BudgetPeriod.Monthly,
            StartDate = startDate,
            EndDate = startDate.AddMonths(1).AddSeconds(-1),
            IsActive = true
        };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, new[] { existingBudget });

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // Test: Creates budget for different category even if budget exists for another category
    [Fact]
    public async Task Handle_WithDifferentCategory_CreatesBudgetSuccessfully()
    {
        var startDate = DateTime.UtcNow;
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 2,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = startDate
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category1 = new Category { Id = 1, Name = "Food", IsActive = true };
        var category2 = new Category { Id = 2, Name = "Transport", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var existingBudget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Period = BudgetPeriod.Monthly,
            StartDate = startDate,
            EndDate = startDate.AddMonths(1).AddSeconds(-1),
            IsActive = true
        };

        SetupMockDbSets(new[] { user }, new[] { category1, category2 }, new[] { currency }, new[] { existingBudget });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.CategoryId.Should().Be(2);
    }

    // Test: Sets default alert thresholds
    [Fact]
    public async Task Handle_SetsDefaultAlertThresholds()
    {
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, Array.Empty<Budget>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AlertAt80Percent.Should().Be(0.8m);
        result.AlertAt100Percent.Should().Be(1.0m);
        result.AlertsEnabled.Should().BeTrue();
    }

    // Test: Creates budget with Weekly period successfully (Step 5 from UC6)
    [Fact]
    public async Task Handle_WithWeeklyPeriod_CreatesBudgetSuccessfully()
    {
        var startDate = new DateTime(2024, 1, 1);
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 200m,
            Period = BudgetPeriod.Weekly,
            StartDate = startDate,
            Description = "Weekly transport budget"
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Transport", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, Array.Empty<Budget>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Amount.Should().Be(200m);
        result.Period.Should().Be(BudgetPeriod.Weekly);
        result.StartDate.Should().Be(startDate);
        _mockContext.Verify(x => x.Add(It.IsAny<Budget>()), Times.Once);
    }

    // Test: Creates budget with Yearly period successfully (Step 5 from UC6)
    [Fact]
    public async Task Handle_WithYearlyPeriod_CreatesBudgetSuccessfully()
    {
        var startDate = new DateTime(2024, 1, 1);
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 12000m,
            Period = BudgetPeriod.Yearly,
            StartDate = startDate,
            Description = "Yearly entertainment budget"
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Entertainment", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, Array.Empty<Budget>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Amount.Should().Be(12000m);
        result.Period.Should().Be(BudgetPeriod.Yearly);
        result.StartDate.Should().Be(startDate);
        _mockContext.Verify(x => x.Add(It.IsAny<Budget>()), Times.Once);
    }

    // Test: Prevents overlapping budgets for same category in Weekly period
    [Fact]
    public async Task Handle_WithWeeklyPeriodOverlap_ThrowsValidationException()
    {
        var startDate = new DateTime(2024, 1, 1);
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 150m,
            Period = BudgetPeriod.Weekly,
            StartDate = startDate
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var existingBudget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Period = BudgetPeriod.Weekly,
            StartDate = startDate,
            EndDate = startDate.AddDays(7).AddSeconds(-1),
            IsActive = true
        };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, new[] { existingBudget });

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // Test: Prevents overlapping budgets for same category in Yearly period
    [Fact]
    public async Task Handle_WithYearlyPeriodOverlap_ThrowsValidationException()
    {
        var startDate = new DateTime(2024, 1, 1);
        var command = new CreateBudgetCommand
        {
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 15000m,
            Period = BudgetPeriod.Yearly,
            StartDate = startDate
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        var existingBudget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Period = BudgetPeriod.Yearly,
            StartDate = startDate,
            EndDate = startDate.AddYears(1).AddSeconds(-1),
            IsActive = true
        };

        SetupMockDbSets(new[] { user }, new[] { category }, new[] { currency }, new[] { existingBudget });

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    private void SetupMockDbSets(
        ApplicationUser[] users,
        Category[] categories,
        Currency[] currencies,
        Budget[] budgets)
    {
        var budgetList = budgets.ToList();

        _mockContext.Setup(x => x.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Categories).Returns(categories.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Currencies).Returns(currencies.AsQueryable().BuildMockDbSet().Object);

        var mockBudgetDbSet = budgetList.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Budgets).Returns(mockBudgetDbSet.Object);

        _mockContext.Setup(x => x.Add(It.IsAny<Budget>()))
            .Callback<Budget>(budget =>
            {
                budget.Id = budgetList.Count + 1;
                budget.Category = categories.FirstOrDefault(c => c.Id == budget.CategoryId);
                budget.Currency = currencies.FirstOrDefault(c => c.Id == budget.CurrencyId);
                budgetList.Add(budget);
                mockBudgetDbSet = budgetList.AsQueryable().BuildMockDbSet();
                _mockContext.Setup(x => x.Budgets).Returns(mockBudgetDbSet.Object);
            });
    }
}
