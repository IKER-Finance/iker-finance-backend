using MediatR;

namespace IkerFinance.Application.Features.Budgets.Queries.GetActiveBudgets;

public class GetActiveBudgetsQuery : IRequest<ActiveBudgetsSummaryDto>
{
    public string UserId { get; set; } = string.Empty;
    public bool IncludeCategories { get; set; } = true;
}
