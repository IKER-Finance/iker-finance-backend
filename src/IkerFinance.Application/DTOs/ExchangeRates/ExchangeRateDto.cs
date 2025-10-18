namespace IkerFinance.Application.DTOs.ExchangeRates;

public class ExchangeRateDto
{
    public int Id { get; set; }
    public int FromCurrencyId { get; set; }
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string FromCurrencyName { get; set; } = string.Empty;
    public int ToCurrencyId { get; set; }
    public string ToCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyName { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? UpdatedByUserId { get; set; }
    public string? UpdatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}
