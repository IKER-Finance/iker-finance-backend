using IkerFinance.Domain.Common;

namespace IkerFinance.Domain.Entities;

public class BudgetCategory : AuditableEntity
{
    public int BudgetId { get; set; }
    public virtual Budget Budget { get; set; } = null!;
    
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    
    public decimal Amount { get; set; }
}