using IkerFinance.Domain.Common;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Domain.Entities;

public class Transaction : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
    
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public virtual Currency Currency { get; set; } = null!;
    
    public decimal ConvertedAmount { get; set; }
    public int ConvertedCurrencyId { get; set; }
    public virtual Currency ConvertedCurrency { get; set; } = null!;
    
    public decimal ExchangeRate { get; set; } = 1.0m;
    public DateTime ExchangeRateDate { get; set; }
    
    public TransactionType Type { get; set; } = TransactionType.Expense;
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime Date { get; set; }
    
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
}