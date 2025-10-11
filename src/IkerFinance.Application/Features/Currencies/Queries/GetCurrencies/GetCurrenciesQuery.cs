using MediatR;
using IkerFinance.Application.DTOs.Currencies;

namespace IkerFinance.Application.Features.Currencies.Queries.GetCurrencies;

public class GetCurrenciesQuery : IRequest<List<CurrencyDto>>
{
}