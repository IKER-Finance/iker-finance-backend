using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.ExchangeRates;

namespace IkerFinance.Application.Common.Interfaces;

public interface IExchangeRateRepository
{
    Task<PaginatedResponse<ExchangeRateDto>> GetExchangeRatesAsync(
        ExchangeRateFilters filters,
        CancellationToken cancellationToken = default);
}
