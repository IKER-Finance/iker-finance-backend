using MediatR;

namespace IkerFinance.Application.Features.Budgets.Queries.PreviewBudgetImpact;

public class PreviewBudgetImpactQuery : IRequest<BudgetImpactPreviewDto>
{
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public DateTime TransactionDate { get; set; }
    public int CurrencyId { get; set; }
}
