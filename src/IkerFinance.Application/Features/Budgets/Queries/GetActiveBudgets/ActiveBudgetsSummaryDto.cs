namespace IkerFinance.Application.Features.Budgets.Queries.GetActiveBudgets;

public class ActiveBudgetsSummaryDto
{
    // Overall Statistics (in home currency)
    public int TotalBudgets { get; set; }
    public decimal TotalBudgetedAmount { get; set; } // Converted to home currency
    public decimal TotalSpentAmount { get; set; } // In home currency
    public int BudgetsOnTrack { get; set; }
    public int BudgetsWarning { get; set; }
    public int BudgetsOverBudget { get; set; }

    public string HomeCurrencyCode { get; set; } = string.Empty;
    public string HomeCurrencySymbol { get; set; } = string.Empty;

    // Individual Budget Summaries
    public List<ActiveBudgetItemDto> Budgets { get; set; } = new();
}

public class ActiveBudgetItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal PercentageSpent { get; set; }
    public string Status { get; set; } = "OnTrack";
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Optional category breakdown
    public List<ActiveBudgetCategoryDto>? Categories { get; set; }
}

public class ActiveBudgetCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal AllocatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal PercentageSpent { get; set; }
}
