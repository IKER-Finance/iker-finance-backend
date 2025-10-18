namespace IkerFinance.Application.DTOs.ExchangeRates;

public class ExchangeRateFilters
{
    public string? SearchTerm { get; set; }
    public int? FromCurrencyId { get; set; }
    public int? ToCurrencyId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? EffectiveDateFrom { get; set; }
    public DateTime? EffectiveDateTo { get; set; }
    public string SortBy { get; set; } = "EffectiveDate";
    public string SortOrder { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
