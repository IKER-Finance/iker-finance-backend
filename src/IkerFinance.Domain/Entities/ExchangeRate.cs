using IkerFinance.Domain.Common;

namespace IkerFinance.Domain.Entities;

public class ExchangeRate : AuditableEntity
{
    public int FromCurrencyId { get; set; }
    public virtual Currency FromCurrency { get; set; } = null!;
    
    public int ToCurrencyId { get; set; }
    public virtual Currency ToCurrency { get; set; } = null!;
    
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    public DateTime? LastUpdated { get; set; }
    public string? UpdatedByUserId { get; set; }
    public virtual ApplicationUser? UpdatedByUser { get; set; }
}