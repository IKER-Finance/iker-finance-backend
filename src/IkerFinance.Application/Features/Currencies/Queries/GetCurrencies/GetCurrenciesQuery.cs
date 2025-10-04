using MediatR;
using IkerFinance.Shared.DTOs.Currencies;

namespace IkerFinance.Application.Features.Currencies.Queries.GetCurrencies;

public class GetCurrenciesQuery : IRequest<List<CurrencyDto>>
{
}