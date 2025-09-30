using IkerFinance.Domain.Common;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Domain.Entities;

public class Category : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#007bff";
    public string Icon { get; set; } = "shopping-cart";
    public TransactionType Type { get; set; } = TransactionType.Expense;
    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    
    public string? UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }
    
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<BudgetCategory> BudgetCategories { get; set; } = new List<BudgetCategory>();
}