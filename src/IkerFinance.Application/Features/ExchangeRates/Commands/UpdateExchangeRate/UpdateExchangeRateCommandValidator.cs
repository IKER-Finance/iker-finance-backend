using FluentValidation;

namespace IkerFinance.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;

public class UpdateExchangeRateCommandValidator : AbstractValidator<UpdateExchangeRateCommand>
{
    public UpdateExchangeRateCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0");

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
