using FluentAssertions;
using IkerFinance.Application.Features.Budgets.Commands.CreateBudget;
using IkerFinance.Domain.Enums;

namespace IkerFinance.UnitTests.Features.Budgets.Validators;

public class CreateBudgetCommandValidatorTests
{
    private readonly CreateBudgetCommandValidator _validator;

    public CreateBudgetCommandValidatorTests()
    {
        _validator = new CreateBudgetCommandValidator();
    }

    // Test: Valid command passes validation
    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = "Monthly food budget"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test: CategoryId is required (Step 7 from UC6)
    [Fact]
    public async Task Validate_WithInvalidCategoryId_ShouldFail()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 0,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "CategoryId")
            .Which.ErrorMessage.Should().Be("Category is required");
    }

    // Test: Amount must be greater than zero (Extension 9a from UC6)
    [Fact]
    public async Task Validate_WithZeroAmount_ShouldFail()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 0m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Amount")
            .Which.ErrorMessage.Should().Be("Budget amount must be greater than zero");
    }

    // Test: Negative amount should fail (Extension 9a from UC6)
    [Fact]
    public async Task Validate_WithNegativeAmount_ShouldFail()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 1,
            CurrencyId = 1,
            Amount = -500m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Amount")
            .Which.ErrorMessage.Should().Be("Budget amount must be greater than zero");
    }

    // Test: CurrencyId is required
    [Fact]
    public async Task Validate_WithInvalidCurrencyId_ShouldFail()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 1,
            CurrencyId = 0,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "CurrencyId")
            .Which.ErrorMessage.Should().Be("Currency is required");
    }

    // Test: StartDate is required (Step 5 from UC6)
    [Fact]
    public async Task Validate_WithEmptyStartDate_ShouldFail()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = default
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "StartDate")
            .Which.ErrorMessage.Should().Be("Start date is required");
    }

    // Test: Period must be valid enum value (Step 5 from UC6)
    [Fact]
    public async Task Validate_WithValidPeriod_ShouldPass()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Weekly,
            StartDate = DateTime.UtcNow
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Period with Monthly should pass (Step 5 from UC6)
    [Fact]
    public async Task Validate_WithMonthlyPeriod_ShouldPass()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Period with Yearly should pass (Step 5 from UC6)
    [Fact]
    public async Task Validate_WithYearlyPeriod_ShouldPass()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 5000m,
            Period = BudgetPeriod.Yearly,
            StartDate = DateTime.UtcNow
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Description can be empty (optional field from Step 5 UC6)
    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldPass()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = null
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Description cannot exceed 500 characters
    [Fact]
    public async Task Validate_WithDescriptionTooLong_ShouldFail()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = new string('a', 501)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Description")
            .Which.ErrorMessage.Should().Be("Description cannot exceed 500 characters");
    }

    // Test: Description with exactly 500 characters should pass
    [Fact]
    public async Task Validate_WithDescriptionExactly500Characters_ShouldPass()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow,
            Description = new string('a', 500)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Valid amount with decimal precision should pass (Step 9 from UC6)
    [Fact]
    public async Task Validate_WithDecimalAmount_ShouldPass()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 1,
            CurrencyId = 1,
            Amount = 1234.56m,
            Period = BudgetPeriod.Monthly,
            StartDate = DateTime.UtcNow
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Multiple validation errors should be reported together
    [Fact]
    public async Task Validate_WithMultipleErrors_ShouldReportAll()
    {
        var command = new CreateBudgetCommand
        {
            UserId = "user123",
            CategoryId = 0,
            CurrencyId = 0,
            Amount = -100m,
            Period = BudgetPeriod.Monthly,
            StartDate = default,
            Description = new string('x', 600)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(5);
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
        result.Errors.Should().Contain(e => e.PropertyName == "CurrencyId");
        result.Errors.Should().Contain(e => e.PropertyName == "StartDate");
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
