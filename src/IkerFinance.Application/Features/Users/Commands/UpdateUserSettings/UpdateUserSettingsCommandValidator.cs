using FluentValidation;

namespace IkerFinance.Application.Features.Users.Commands.UpdateUserSettings;

public class UpdateUserSettingsCommandValidator : AbstractValidator<UpdateUserSettingsCommand>
{
    public UpdateUserSettingsCommandValidator()
    {
        RuleFor(x => x.TimeZone)
            .NotEmpty()
            .WithMessage("TimeZone is required")
            .MaximumLength(50)
            .WithMessage("TimeZone cannot exceed 50 characters");

        RuleFor(x => x.DefaultTransactionCurrencyId)
            .GreaterThan(0)
            .WithMessage("Default transaction currency ID must be greater than 0")
            .When(x => x.DefaultTransactionCurrencyId.HasValue);
    }
}
