using FluentAssertions;
using IkerFinance.Application.Features.Transactions.Commands.CreateTransaction;

namespace IkerFinance.UnitTests.Features.Transactions.Validators;

public class CreateTransactionCommandValidatorTests
{
    private readonly CreateTransactionCommandValidator _validator;

    public CreateTransactionCommandValidatorTests()
    {
        _validator = new CreateTransactionCommandValidator();
    }

    // Test: Valid command passes validation
    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100.50m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow.AddDays(-1),
            Description = "Groceries",
            Notes = "Weekly shopping"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test: Amount must be greater than zero (Extension 3a from UC2)
    [Fact]
    public async Task Validate_WithZeroAmount_ShouldFail()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 0m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Amount")
            .Which.ErrorMessage.Should().Be("Amount must be greater than zero");
    }

    // Test: Negative amount should fail (Extension 3a from UC2)
    [Fact]
    public async Task Validate_WithNegativeAmount_ShouldFail()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = -50m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Amount")
            .Which.ErrorMessage.Should().Be("Amount must be greater than zero");
    }

    // Test: CurrencyId is required (Step 5 from UC2)
    [Fact]
    public async Task Validate_WithInvalidCurrencyId_ShouldFail()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 0,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "CurrencyId")
            .Which.ErrorMessage.Should().Be("Currency is required");
    }

    // Test: CategoryId is required (Step 9 from UC2)
    [Fact]
    public async Task Validate_WithInvalidCategoryId_ShouldFail()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 0,
            Date = DateTime.UtcNow,
            Description = "Test"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "CategoryId")
            .Which.ErrorMessage.Should().Be("Category is required");
    }

    // Test: Description is required (Step 11 from UC2)
    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldFail()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = ""
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Description")
            .Which.ErrorMessage.Should().Be("Description is required");
    }

    // Test: Description cannot exceed 500 characters
    [Fact]
    public async Task Validate_WithDescriptionTooLong_ShouldFail()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
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
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow.AddDays(-1),
            Description = new string('a', 500)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Notes can be empty (optional field from Step 11 UC2)
    [Fact]
    public async Task Validate_WithEmptyNotes_ShouldPass()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow.AddDays(-1),
            Description = "Test",
            Notes = null
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Notes cannot exceed 1000 characters
    [Fact]
    public async Task Validate_WithNotesTooLong_ShouldFail()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow,
            Description = "Test",
            Notes = new string('b', 1001)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Notes")
            .Which.ErrorMessage.Should().Be("Notes cannot exceed 1000 characters");
    }

    // Test: Notes with exactly 1000 characters should pass
    [Fact]
    public async Task Validate_WithNotesExactly1000Characters_ShouldPass()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow.AddDays(-1),
            Description = "Test",
            Notes = new string('b', 1000)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Date is required (Step 7 from UC2)
    [Fact]
    public async Task Validate_WithEmptyDate_ShouldFail()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = default,
            Description = "Test"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Date")
            .Which.ErrorMessage.Should().Be("Transaction date is required");
    }

    // Test: Future date should fail (Extension 7a from UC2)
    [Fact]
    public async Task Validate_WithFutureDate_ShouldFail()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Test"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Date")
            .Which.ErrorMessage.Should().Be("Transaction date cannot be in the future");
    }

    // Test: Current date should pass (Step 7 from UC2)
    [Fact]
    public async Task Validate_WithCurrentDate_ShouldPass()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow.AddMinutes(-1), // Use a time slightly in the past to avoid timing issues
            Description = "Test"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Past date should pass (Step 7 from UC2)
    [Fact]
    public async Task Validate_WithPastDate_ShouldPass()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = 100m,
            CurrencyId = 1,
            CategoryId = 1,
            Date = DateTime.UtcNow.AddDays(-30),
            Description = "Test"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    // Test: Multiple validation errors should be reported together
    [Fact]
    public async Task Validate_WithMultipleErrors_ShouldReportAll()
    {
        var command = new CreateTransactionCommand
        {
            UserId = "user123",
            Amount = -50m,
            CurrencyId = 0,
            CategoryId = 0,
            Date = DateTime.UtcNow.AddDays(5),
            Description = ""
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(5);
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
        result.Errors.Should().Contain(e => e.PropertyName == "CurrencyId");
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
        result.Errors.Should().Contain(e => e.PropertyName == "Date");
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
