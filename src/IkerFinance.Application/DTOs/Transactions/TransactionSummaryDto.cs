namespace IkerFinance.Application.DTOs.Transactions;

public class TransactionSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetAmount { get; set; }
    public int TotalTransactions { get; set; }
    
    public int HomeCurrencyId { get; set; }
    public string HomeCurrencyCode { get; set; } = string.Empty;
    public string HomeCurrencySymbol { get; set; } = string.Empty;
    
    public List<CategorySummary> TopIncomeCategories { get; set; } = new();
    public List<CategorySummary> TopExpenseCategories { get; set; } = new();
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CategorySummary
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal Percentage { get; set; }
}