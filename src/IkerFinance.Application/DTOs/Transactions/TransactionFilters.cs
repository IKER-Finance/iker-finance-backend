using IkerFinance.Domain.Enums;

namespace IkerFinance.Application.DTOs.Transactions;

public class TransactionFilters
{
    public string UserId { get; set; } = string.Empty;
    public string? SearchTerm { get; set; }
    public TransactionType? Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CategoryId { get; set; }
    public int? CurrencyId { get; set; }
    public string SortBy { get; set; } = "Date";
    public string SortOrder { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
