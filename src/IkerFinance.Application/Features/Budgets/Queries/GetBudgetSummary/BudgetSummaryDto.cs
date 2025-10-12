namespace IkerFinance.Application.Features.Budgets.Queries.GetBudgetSummary;

public class BudgetSummaryDto
{
    public int BudgetId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public string? CategoryColor { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;

    // Spending Analytics
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal PercentageSpent { get; set; }
    public string Status { get; set; } = "OnTrack"; // OnTrack, Warning, OverBudget

    // Budget Info
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }

    // Statistics
    public int TransactionCount { get; set; }
    public bool AlertAt80Percent { get; set; }
    public bool AlertAt100Percent { get; set; }
}
