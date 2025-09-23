using IkerFinance.Domain.Enums;

namespace IkerFinance.Domain.Entities;

public class Budget
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
}