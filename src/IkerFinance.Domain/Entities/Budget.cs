using IkerFinance.Domain.Common;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Domain.Entities;

public class Budget : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public BudgetPeriod Period { get; set; } = BudgetPeriod.Monthly;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public virtual Currency Currency { get; set; } = null!;
    
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public bool AllowOverlap { get; set; } = false;
    public decimal AlertAt80Percent { get; set; } = 0.8m;
    public decimal AlertAt100Percent { get; set; } = 1.0m;
    public bool AlertsEnabled { get; set; } = true;
    
    public virtual ICollection<BudgetCategory> Categories { get; set; } = new List<BudgetCategory>();
}