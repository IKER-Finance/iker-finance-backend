using IkerFinance.Application.Common.Queries;
using IkerFinance.Shared.DTOs.Common;
using IkerFinance.Shared.DTOs.Transactions;

namespace IkerFinance.Application.Features.Transactions.Queries.GetTransactions;

public class GetTransactionsQuery : SearchableQuery<PaginatedResponse<TransactionDto>>
{
    public string UserId { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CategoryId { get; set; }
    public int? CurrencyId { get; set; }
    public int? Type { get; set; }
}