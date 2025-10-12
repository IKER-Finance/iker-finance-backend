using FluentValidation;

namespace IkerFinance.Application.Features.Budgets.Commands.CreateBudget;

public class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Category is required");

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
    }
}