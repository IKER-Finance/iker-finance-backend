using MediatR;
using IkerFinance.Domain.Enums;
using IkerFinance.Application.DTOs.Budgets;

namespace IkerFinance.Application.Features.Budgets.Commands.UpdateBudget;

public class UpdateBudgetCommand : IRequest<BudgetDto>
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}