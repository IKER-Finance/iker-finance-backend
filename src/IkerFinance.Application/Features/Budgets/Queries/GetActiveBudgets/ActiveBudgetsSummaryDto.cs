namespace IkerFinance.Application.Features.Budgets.Queries.GetActiveBudgets;

public class ActiveBudgetsSummaryDto
{
    public int TotalBudgets { get; set; }
    public decimal TotalBudgetedAmount { get; set; }
    public decimal TotalSpentAmount { get; set; }
    public int BudgetsOnTrack { get; set; }
    public int BudgetsWarning { get; set; }
    public int BudgetsOverBudget { get; set; }
    public string HomeCurrencyCode { get; set; } = string.Empty;
    public string HomeCurrencySymbol { get; set; } = string.Empty;
    public List<ActiveBudgetItemDto> Budgets { get; set; } = new();
}

public class ActiveBudgetItemDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public string? CategoryColor { get; set; }
    public decimal Amount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal PercentageSpent { get; set; }
    public string Status { get; set; } = "OnTrack";
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
