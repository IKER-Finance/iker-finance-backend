using IkerFinance.Domain.Enums;

namespace IkerFinance.Application.DTOs.Budgets;

public class BudgetDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public string? CategoryColor { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public bool AllowOverlap { get; set; }
    public decimal AlertAt80Percent { get; set; }
    public decimal AlertAt100Percent { get; set; }
    public bool AlertsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}