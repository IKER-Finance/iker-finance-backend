namespace IkerFinance.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public decimal ConvertedAmount { get; set; }
    public Currency HomeCurrency { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
}