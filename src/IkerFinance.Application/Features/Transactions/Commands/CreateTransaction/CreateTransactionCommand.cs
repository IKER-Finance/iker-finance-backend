using IkerFinance.Shared.DTOs.Transactions;
using MediatR;

namespace IkerFinance.Application.Features.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommand : IRequest<TransactionDto>
{
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public int CategoryId { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
}