using IkerFinance.Application.Common.Queries;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.ExchangeRates;

namespace IkerFinance.Application.Features.ExchangeRates.Queries.GetExchangeRates;

public class GetExchangeRatesQuery : SearchableQuery<PaginatedResponse<ExchangeRateDto>>
{
    public int? FromCurrencyId { get; set; }
    public int? ToCurrencyId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? EffectiveDateFrom { get; set; }
    public DateTime? EffectiveDateTo { get; set; }
}
