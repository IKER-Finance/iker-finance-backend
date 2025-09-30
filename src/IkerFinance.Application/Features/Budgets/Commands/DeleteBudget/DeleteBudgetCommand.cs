using MediatR;

namespace IkerFinance.Application.Features.Budgets.Commands.DeleteBudget;

public class DeleteBudgetCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
}