using IkerFinance.Application.DTOs.ExchangeRates;
using MediatR;

namespace IkerFinance.Application.Features.ExchangeRates.Queries.GetExchangeRateById;

public class GetExchangeRateByIdQuery : IRequest<ExchangeRateDto>
{
    public int Id { get; set; }
}
