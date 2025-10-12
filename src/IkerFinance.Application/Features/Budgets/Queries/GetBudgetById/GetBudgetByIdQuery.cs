using MediatR;
using IkerFinance.Application.DTOs.Budgets;

namespace IkerFinance.Application.Features.Budgets.Queries.GetBudgetById;

public class GetBudgetByIdQuery : IRequest<BudgetDto>
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
}