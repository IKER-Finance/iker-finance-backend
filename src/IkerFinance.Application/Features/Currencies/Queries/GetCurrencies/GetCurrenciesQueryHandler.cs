using MediatR;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Currencies;
using IkerFinance.Domain.Entities;

namespace IkerFinance.Application.Features.Currencies.Queries.GetCurrencies;

public sealed class GetCurrenciesQueryHandler : IRequestHandler<GetCurrenciesQuery, List<CurrencyDto>>
{
    private readonly IReadRepository<Currency> _currencyRepository;

    public GetCurrenciesQueryHandler(IReadRepository<Currency> currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<List<CurrencyDto>> Handle(
        GetCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        var currencies = await _currencyRepository.FindAsync(
            c => c.IsActive,
            cancellationToken);

        return currencies
            .OrderBy(c => c.Code)
            .Select(c => new CurrencyDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Symbol = c.Symbol
            })
            .ToList();
    }
}