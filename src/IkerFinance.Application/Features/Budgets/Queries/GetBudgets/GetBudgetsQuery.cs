using IkerFinance.Application.Common.Queries;
using IkerFinance.Domain.Enums;
using IkerFinance.Shared.DTOs.Common;
using IkerFinance.Shared.DTOs.Budgets;

namespace IkerFinance.Application.Features.Budgets.Queries.GetBudgets;

public class GetBudgetsQuery : SearchableQuery<PaginatedResponse<BudgetDto>>
{
    public string UserId { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
    public BudgetPeriod? Period { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}