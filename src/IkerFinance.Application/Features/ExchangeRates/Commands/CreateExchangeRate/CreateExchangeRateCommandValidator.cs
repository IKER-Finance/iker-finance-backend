using FluentValidation;

namespace IkerFinance.Application.Features.ExchangeRates.Commands.CreateExchangeRate;

public class CreateExchangeRateCommandValidator : AbstractValidator<CreateExchangeRateCommand>
{
    public CreateExchangeRateCommandValidator()
    {
        RuleFor(x => x.FromCurrencyId)
            .GreaterThan(0)
            .WithMessage("FromCurrencyId must be greater than 0");

        RuleFor(x => x.ToCurrencyId)
            .GreaterThan(0)
            .WithMessage("ToCurrencyId must be greater than 0");

        RuleFor(x => x)
            .Must(x => x.FromCurrencyId != x.ToCurrencyId)
            .WithMessage("FromCurrencyId and ToCurrencyId cannot be the same");

        RuleFor(x => x.Rate)
            .GreaterThan(0)
            .WithMessage("Rate must be greater than 0")
            .LessThan(1000000)
            .WithMessage("Rate cannot exceed 1,000,000");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty()
            .WithMessage("EffectiveDate is required");

        RuleFor(x => x.AdminUserId)
            .NotEmpty()
            .WithMessage("AdminUserId is required");
    }
}
