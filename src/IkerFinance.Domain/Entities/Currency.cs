using IkerFinance.Domain.Common;

namespace IkerFinance.Domain.Entities;

public class Currency : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; } = 2;
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<ExchangeRate> FromExchangeRates { get; set; } = new List<ExchangeRate>();
    public virtual ICollection<ExchangeRate> ToExchangeRates { get; set; } = new List<ExchangeRate>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}