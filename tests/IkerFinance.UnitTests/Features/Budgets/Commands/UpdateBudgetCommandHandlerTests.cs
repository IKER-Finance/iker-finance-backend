using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Budgets.Commands.UpdateBudget;
using IkerFinance.Domain.DomainServices.Budget;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.Budgets.Commands;

public class UpdateBudgetCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrencyConversionService> _mockConversionService;
    private readonly BudgetService _budgetService;
    private readonly UpdateBudgetCommandHandler _handler;
    private const string TestUserId = "user123";

    public UpdateBudgetCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockConversionService = new Mock<ICurrencyConversionService>();
        _budgetService = new BudgetService();
        _handler = new UpdateBudgetCommandHandler(
            _mockContext.Object,
            _mockConversionService.Object,
            _budgetService);
    }

    // Test: Updates budget successfully with valid data
    [Fact]
    public async Task Handle_WithValidData_UpdatesBudgetSuccessfully()
    {
        var existingBudget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 500m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow.AddDays(-10)
        };

        var command = new UpdateBudgetCommand
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1500m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = "Updated budget",
            IsActive = true
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { existingBudget }, new[] { user }, new[] { category }, new[] { currency });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Amount.Should().Be(1500m);
        result.Description.Should().Be("Updated budget");
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    // Test: Throws NotFoundException when budget does not exist
    [Fact]
    public async Task Handle_WhenBudgetDoesNotExist_ThrowsNotFoundException()
    {
        var command = new UpdateBudgetCommand
        {
            Id = 999,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        SetupMockDbSets(Array.Empty<Budget>(), Array.Empty<ApplicationUser>(), Array.Empty<Category>(), Array.Empty<Currency>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Budget*999*");
    }

    // Test: Throws NotFoundException when budget belongs to different user
    [Fact]
    public async Task Handle_WhenBudgetBelongsToDifferentUser_ThrowsNotFoundException()
    {
        var existingBudget = new Budget
        {
            Id = 1,
            UserId = "otherUser",
            CategoryId = 1,
            Amount = 1000m
        };

        var command = new UpdateBudgetCommand
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1500m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        SetupMockDbSets(new[] { existingBudget }, Array.Empty<ApplicationUser>(), Array.Empty<Category>(), Array.Empty<Currency>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Budget*1*");
    }

    // Test: Throws ValidationException when currency is inactive
    [Fact]
    public async Task Handle_WhenCurrencyIsInactive_ThrowsValidationException()
    {
        var existingBudget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Amount = 1000m
        };

        var command = new UpdateBudgetCommand
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1500m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = false };

        SetupMockDbSets(new[] { existingBudget }, new[] { user }, Array.Empty<Category>(), new[] { currency });

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // Test: Throws NotFoundException when category does not exist
    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ThrowsNotFoundException()
    {
        var existingBudget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Amount = 1000m
        };

        var command = new UpdateBudgetCommand
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 999,
            CurrencyId = 1,
            Amount = 1500m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { existingBudget }, new[] { user }, Array.Empty<Category>(), new[] { currency });

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Category*999*");
    }

    // Test: Throws ValidationException when exchange rate not available for different currency
    [Fact]
    public async Task Handle_WithDifferentCurrencyNoRate_ThrowsValidationException()
    {
        var existingBudget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m
        };

        var command = new UpdateBudgetCommand
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 2,
            Amount = 1500m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", IsActive = true };
        var currency = new Currency { Id = 2, Code = "EUR", Symbol = "€", IsActive = true };

        SetupMockDbSets(new[] { existingBudget }, new[] { user }, new[] { category }, new[] { currency });

        _mockConversionService.Setup(x => x.RatesExistAsync(2, 1)).ReturnsAsync(false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // Test: Updates budget with different currency when exchange rate exists
    [Fact]
    public async Task Handle_WithDifferentCurrencyAndRate_UpdatesBudgetSuccessfully()
    {
        var existingBudget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var command = new UpdateBudgetCommand
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 2,
            Amount = 1500m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", IsActive = true };
        var homeCurrency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };
        var currency = new Currency { Id = 2, Code = "EUR", Symbol = "€", IsActive = true };

        SetupMockDbSets(new[] { existingBudget }, new[] { user }, new[] { category }, new[] { homeCurrency, currency });

        _mockConversionService.Setup(x => x.RatesExistAsync(2, 1)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.CurrencyCode.Should().Be("EUR");
    }

    // Test: Throws ValidationException when changing category to one with existing overlapping budget
    [Fact]
    public async Task Handle_WhenChangingCategoryWithOverlap_ThrowsValidationException()
    {
        var startDate = new DateTime(2024, 1, 1);
        var existingBudget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            Period = BudgetPeriod.Monthly,
            StartDate = startDate,
            EndDate = startDate.AddMonths(1).AddSeconds(-1)
        };

        var otherBudget = new Budget
        {
            Id = 2,
            UserId = TestUserId,
            CategoryId = 2,
            Period = BudgetPeriod.Monthly,
            StartDate = startDate,
            EndDate = startDate.AddMonths(1).AddSeconds(-1),
            IsActive = true
        };

        var command = new UpdateBudgetCommand
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 2,
            CurrencyId = 1,
            Amount = 1500m,
            Period = BudgetPeriod.Monthly,
            StartDate = startDate
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 2, Name = "Transport", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { existingBudget, otherBudget }, new[] { user }, new[] { category }, new[] { currency });

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // Test: Updates budget to inactive status
    [Fact]
    public async Task Handle_UpdatesIsActiveStatus()
    {
        var existingBudget = new Budget
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            IsActive = true
        };

        var command = new UpdateBudgetCommand
        {
            Id = 1,
            UserId = TestUserId,
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            IsActive = false
        };

        var user = new ApplicationUser { Id = TestUserId, HomeCurrencyId = 1 };
        var category = new Category { Id = 1, Name = "Food", IsActive = true };
        var currency = new Currency { Id = 1, Code = "USD", Symbol = "$", IsActive = true };

        SetupMockDbSets(new[] { existingBudget }, new[] { user }, new[] { category }, new[] { currency });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsActive.Should().BeFalse();
    }

    private void SetupMockDbSets(
        Budget[] budgets,
        ApplicationUser[] users,
        Category[] categories,
        Currency[] currencies)
    {
        var budgetList = budgets.ToList();

        foreach (var budget in budgetList)
        {
            budget.Category = categories.FirstOrDefault(c => c.Id == budget.CategoryId);
            budget.Currency = currencies.FirstOrDefault(c => c.Id == budget.CurrencyId);
        }

        var mockBudgetDbSet = budgetList.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Budgets).Returns(mockBudgetDbSet.Object);
        _mockContext.Setup(x => x.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Categories).Returns(categories.AsQueryable().BuildMockDbSet().Object);
        _mockContext.Setup(x => x.Currencies).Returns(currencies.AsQueryable().BuildMockDbSet().Object);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                foreach (var budget in budgetList)
                {
                    budget.Category = categories.FirstOrDefault(c => c.Id == budget.CategoryId);
                    budget.Currency = currencies.FirstOrDefault(c => c.Id == budget.CurrencyId);
                }
                mockBudgetDbSet = budgetList.AsQueryable().BuildMockDbSet();
                _mockContext.Setup(x => x.Budgets).Returns(mockBudgetDbSet.Object);
            })
            .ReturnsAsync(1);
    }
}
