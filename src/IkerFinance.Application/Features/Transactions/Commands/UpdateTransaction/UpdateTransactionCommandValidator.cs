using FluentValidation;

namespace IkerFinance.Application.Features.Transactions.Commands.UpdateTransaction;

public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Transaction ID must be valid");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0)
            .WithMessage("Currency is required");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Category is required");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Transaction date is required")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Transaction date cannot be in the future");
    }
}