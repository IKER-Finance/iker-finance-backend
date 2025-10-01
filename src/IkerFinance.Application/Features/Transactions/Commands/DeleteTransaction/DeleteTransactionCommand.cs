using MediatR;

namespace IkerFinance.Application.Features.Transactions.Commands.DeleteTransaction;

public class DeleteTransactionCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
}