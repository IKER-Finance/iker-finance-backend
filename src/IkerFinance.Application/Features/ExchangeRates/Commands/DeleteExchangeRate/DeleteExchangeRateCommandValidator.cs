using FluentValidation;

namespace IkerFinance.Application.Features.ExchangeRates.Commands.DeleteExchangeRate;

public class DeleteExchangeRateCommandValidator : AbstractValidator<DeleteExchangeRateCommand>
{
    public DeleteExchangeRateCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0");
    }
}
