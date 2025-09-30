using MediatR;
using IkerFinance.Shared.DTOs.Transactions;

namespace IkerFinance.Application.Features.Transactions.Commands.UpdateTransaction;

public class UpdateTransactionCommand : IRequest<TransactionDto>
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public int CategoryId { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
}