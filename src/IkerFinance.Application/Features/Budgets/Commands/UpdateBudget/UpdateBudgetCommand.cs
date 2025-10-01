using MediatR;
using IkerFinance.Domain.Enums;
using IkerFinance.Shared.DTOs.Budgets;

namespace IkerFinance.Application.Features.Budgets.Commands.UpdateBudget;

public class UpdateBudgetCommand : IRequest<BudgetDto>
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public List<BudgetCategoryAllocation> CategoryAllocations { get; set; } = new();
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}