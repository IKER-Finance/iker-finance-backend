using MediatR;

namespace IkerFinance.Application.Features.ExchangeRates.Commands.DeleteExchangeRate;

public class DeleteExchangeRateCommand : IRequest<Unit>
{
    public int Id { get; set; }
}
