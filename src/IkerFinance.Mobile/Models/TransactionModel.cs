namespace IkerFinance.Mobile.Models;

public class TransactionModel
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public bool IsIncome { get; set; }

    // Display properties
    public string DisplayAmount => $"{(IsIncome ? "+" : "-")}{CurrencyCode} {Amount:N2}";
    public string DisplayDate => Date.ToString("MMM dd, yyyy");
    public string AmountColor => IsIncome ? "#10b981" : "#ef4444"; // Green for income, red for expense
}
