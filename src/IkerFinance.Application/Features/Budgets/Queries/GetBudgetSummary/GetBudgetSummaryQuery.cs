using MediatR;

namespace IkerFinance.Application.Features.Budgets.Queries.GetBudgetSummary;

public class GetBudgetSummaryQuery : IRequest<BudgetSummaryDto>
{
    public int BudgetId { get; set; }
    public string UserId { get; set; } = string.Empty;
}
