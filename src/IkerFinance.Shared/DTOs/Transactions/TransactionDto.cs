using IkerFinance.Domain.Enums;

namespace IkerFinance.Shared.DTOs.Transactions;

public class TransactionDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public decimal ConvertedAmount { get; set; }
    public int ConvertedCurrencyId { get; set; }
    public string ConvertedCurrencyCode { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public TransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}