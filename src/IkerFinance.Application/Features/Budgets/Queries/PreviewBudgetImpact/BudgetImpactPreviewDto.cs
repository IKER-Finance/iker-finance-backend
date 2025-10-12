namespace IkerFinance.Application.Features.Budgets.Queries.PreviewBudgetImpact;

public class BudgetImpactPreviewDto
{
    public List<AffectedBudgetDto> AffectedBudgets { get; set; } = new();
    public bool HasWarnings { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class AffectedBudgetDto
{
    public int BudgetId { get; set; }
    public string BudgetName { get; set; } = string.Empty;

    // Current State
    public decimal CurrentSpent { get; set; }
    public decimal CurrentRemaining { get; set; }
    public decimal CurrentPercentage { get; set; }
    public string StatusBefore { get; set; } = "OnTrack";

    // After Transaction
    public decimal AfterSpent { get; set; }
    public decimal AfterRemaining { get; set; }
    public decimal AfterPercentage { get; set; }
    public string StatusAfter { get; set; } = "OnTrack";

    // Alerts
    public bool WillTriggerAlert { get; set; }
    public string? AlertMessage { get; set; }

    // Currency Info
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;

    // Category Impact (if budget has category allocation)
    public AffectedCategoryDto? AffectedCategory { get; set; }
}

public class AffectedCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Allocated { get; set; }
    public decimal CurrentSpent { get; set; }
    public decimal AfterSpent { get; set; }
    public bool WillExceed { get; set; }
    public decimal ExceedAmount { get; set; }
}
