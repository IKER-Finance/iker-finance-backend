using IkerFinance.Domain.Enums;

namespace IkerFinance.Application.DTOs.Budgets;

public class BudgetFilters
{
    public string UserId { get; set; } = string.Empty;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public BudgetPeriod? Period { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string SortBy { get; set; } = "StartDate";
    public string SortOrder { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
