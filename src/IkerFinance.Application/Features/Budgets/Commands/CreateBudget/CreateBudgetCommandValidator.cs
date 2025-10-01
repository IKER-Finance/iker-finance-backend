using FluentValidation;

namespace IkerFinance.Application.Features.Budgets.Commands.CreateBudget;

public class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Budget name is required")
            .MaximumLength(100)
            .WithMessage("Budget name cannot exceed 100 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Budget amount must be greater than zero");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0)
            .WithMessage("Currency is required");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        RuleFor(x => x.Period)
            .IsInEnum()
            .WithMessage("Invalid budget period");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.CategoryAllocations)
            .Must(allocations => allocations.All(a => a.Amount > 0))
            .WithMessage("All category allocations must have amounts greater than zero")
            .When(x => x.CategoryAllocations.Any());

        RuleFor(x => x.CategoryAllocations)
            .Must((command, allocations) => allocations.Sum(a => a.Amount) <= command.Amount)
            .WithMessage("Total category allocations cannot exceed budget amount")
            .When(x => x.CategoryAllocations.Any());
    }
}