using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.Transactions;

namespace IkerFinance.Application.Common.Interfaces;

public interface ITransactionRepository
{
    Task<PaginatedResponse<TransactionDto>> GetTransactionsWithDetailsAsync(
        TransactionFilters filters,
        CancellationToken cancellationToken = default);

    Task<TransactionDto?> GetTransactionWithDetailsAsync(
        int id,
        string userId,
        CancellationToken cancellationToken = default);
}
