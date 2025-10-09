using FluentAssertions;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using IkerFinance.Domain.Services;

namespace IkerFinance.UnitTests.Domain.Services;

public class BudgetServiceTests
{
    private readonly BudgetService _service;

    public BudgetServiceTests()
    {
        _service = new BudgetService();
    }

    // Test: Daily budget period adds 1 day to start date
    [Fact]
    public void Create_WithDailyPeriod_CalculatesEndDateCorrectly()
    {
        var startDate = new DateTime(2025, 1, 1);

        var result = _service.Create(
            userId: "user123",
            name: "Daily Budget",
            currencyId: 1,
            amount: 100m,
            period: BudgetPeriod.Daily,
            startDate: startDate,
            description: "Test"
        );

        result.StartDate.Should().Be(startDate);
        result.EndDate.Should().Be(new DateTime(2025, 1, 2));
    }

    // Test: Weekly budget period adds 7 days to start date
    [Fact]
    public void Create_WithWeeklyPeriod_CalculatesEndDateCorrectly()
    {
        var startDate = new DateTime(2025, 1, 1);

        var result = _service.Create(
            userId: "user123",
            name: "Weekly Budget",
            currencyId: 1,
            amount: 500m,
            period: BudgetPeriod.Weekly,
            startDate: startDate,
            description: null
        );

        result.StartDate.Should().Be(startDate);
        result.EndDate.Should().Be(new DateTime(2025, 1, 8));
    }

    // Test: Monthly budget period adds 1 month to start date
    [Fact]
    public void Create_WithMonthlyPeriod_CalculatesEndDateCorrectly()
    {
        var startDate = new DateTime(2025, 1, 15);

        var result = _service.Create(
            userId: "user123",
            name: "Monthly Budget",
            currencyId: 1,
            amount: 2000m,
            period: BudgetPeriod.Monthly,
            startDate: startDate,
            description: "Monthly expenses"
        );

        result.StartDate.Should().Be(startDate);
        result.EndDate.Should().Be(new DateTime(2025, 2, 15));
    }

    // Test: Quarterly budget period adds 3 months to start date
    [Fact]
    public void Create_WithQuarterlyPeriod_CalculatesEndDateCorrectly()
    {
        var startDate = new DateTime(2025, 1, 1);

        var result = _service.Create(
            userId: "user123",
            name: "Quarterly Budget",
            currencyId: 1,
            amount: 6000m,
            period: BudgetPeriod.Quarterly,
            startDate: startDate,
            description: null
        );

        result.StartDate.Should().Be(startDate);
        result.EndDate.Should().Be(new DateTime(2025, 4, 1));
    }

    // Test: Yearly budget period adds 1 year to start date
    [Fact]
    public void Create_WithYearlyPeriod_CalculatesEndDateCorrectly()
    {
        var startDate = new DateTime(2025, 3, 15);

        var result = _service.Create(
            userId: "user123",
            name: "Yearly Budget",
            currencyId: 1,
            amount: 24000m,
            period: BudgetPeriod.Yearly,
            startDate: startDate,
            description: "Annual budget"
        );

        result.StartDate.Should().Be(startDate);
        result.EndDate.Should().Be(new DateTime(2026, 3, 15));
    }

    // Test: Budget creation sets default properties correctly
    [Fact]
    public void Create_SetsDefaultProperties()
    {
        var result = _service.Create(
            userId: "user123",
            name: "Test Budget",
            currencyId: 2,
            amount: 1000m,
            period: BudgetPeriod.Monthly,
            startDate: DateTime.UtcNow,
            description: "Description"
        );

        result.UserId.Should().Be("user123");
        result.Name.Should().Be("Test Budget");
        result.CurrencyId.Should().Be(2);
        result.Amount.Should().Be(1000m);
        result.Description.Should().Be("Description");
        result.IsActive.Should().BeTrue();
        result.AllowOverlap.Should().BeFalse();
        result.AlertAt80Percent.Should().Be(0.8m);
        result.AlertAt100Percent.Should().Be(1.0m);
        result.AlertsEnabled.Should().BeTrue();
    }

    // Test: Changing budget period recalculates end date
    [Fact]
    public void Update_ChangingPeriod_RecalculatesEndDate()
    {
        var budget = new Budget
        {
            Id = 1,
            UserId = "user123",
            Name = "Old Budget",
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 2, 1),
            IsActive = true
        };

        _service.Update(
            budget: budget,
            name: "Updated Budget",
            currencyId: 2,
            amount: 2000m,
            period: BudgetPeriod.Quarterly,
            startDate: new DateTime(2025, 1, 1),
            description: "New description",
            isActive: false
        );

        budget.Name.Should().Be("Updated Budget");
        budget.CurrencyId.Should().Be(2);
        budget.Amount.Should().Be(2000m);
        budget.Period.Should().Be(BudgetPeriod.Quarterly);
        budget.EndDate.Should().Be(new DateTime(2025, 4, 1));
        budget.Description.Should().Be("New description");
        budget.IsActive.Should().BeFalse();
    }

    // Test: Changing start date recalculates end date based on period
    [Fact]
    public void Update_ChangingStartDate_RecalculatesEndDate()
    {
        var budget = new Budget
        {
            Id = 1,
            UserId = "user123",
            Name = "Budget",
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 2, 1)
        };

        _service.Update(
            budget: budget,
            name: "Budget",
            currencyId: 1,
            amount: 1000m,
            period: BudgetPeriod.Monthly,
            startDate: new DateTime(2025, 2, 15),
            description: null,
            isActive: true
        );

        budget.StartDate.Should().Be(new DateTime(2025, 2, 15));
        budget.EndDate.Should().Be(new DateTime(2025, 3, 15));
    }

    // Test: Update method sets UpdatedAt timestamp
    [Fact]
    public void Update_SetsUpdatedAtTimestamp()
    {
        var budget = new Budget
        {
            Id = 1,
            UserId = "user123",
            Name = "Budget",
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var beforeUpdate = DateTime.UtcNow;

        _service.Update(
            budget: budget,
            name: "Updated",
            currencyId: 1,
            amount: 1500m,
            period: BudgetPeriod.Monthly,
            startDate: DateTime.UtcNow,
            description: null,
            isActive: true
        );

        budget.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    // Test: Monthly period handles leap year correctly (Jan 31 -> Feb 29 in leap year)
    [Fact]
    public void Create_WithLeapYearMonthly_HandlesCorrectly()
    {
        var startDate = new DateTime(2024, 1, 31);

        var result = _service.Create(
            userId: "user123",
            name: "Leap Year Test",
            currencyId: 1,
            amount: 1000m,
            period: BudgetPeriod.Monthly,
            startDate: startDate,
            description: null
        );

        result.EndDate.Should().Be(new DateTime(2024, 2, 29));
    }
}