using MediatR;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.ExchangeRates;

namespace IkerFinance.Application.Features.ExchangeRates.Queries.GetExchangeRates;

public sealed class GetExchangeRatesQueryHandler : IRequestHandler<GetExchangeRatesQuery, PaginatedResponse<ExchangeRateDto>>
{
    private readonly IExchangeRateRepository _exchangeRateRepository;

    public GetExchangeRatesQueryHandler(IExchangeRateRepository exchangeRateRepository)
    {
        _exchangeRateRepository = exchangeRateRepository;
    }

    public async Task<PaginatedResponse<ExchangeRateDto>> Handle(
        GetExchangeRatesQuery request,
        CancellationToken cancellationToken)
    {
        request.ValidatePagination();

        var filters = new ExchangeRateFilters
        {
            SearchTerm = request.GetNormalizedSearchTerm(),
            FromCurrencyId = request.FromCurrencyId,
            ToCurrencyId = request.ToCurrencyId,
            IsActive = request.IsActive,
            EffectiveDateFrom = request.EffectiveDateFrom,
            EffectiveDateTo = request.EffectiveDateTo,
            SortBy = request.SortBy,
            SortOrder = request.SortOrder,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return await _exchangeRateRepository.GetExchangeRatesAsync(filters, cancellationToken);
    }
}
