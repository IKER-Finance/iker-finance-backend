using MediatR;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.Transactions;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Application.Features.Transactions.Queries.GetTransactions;

public sealed class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PaginatedResponse<TransactionDto>>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionsQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<PaginatedResponse<TransactionDto>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        request.ValidatePagination();

        var filters = new TransactionFilters
        {
            UserId = request.UserId,
            SearchTerm = request.GetNormalizedSearchTerm(),
            Type = request.Type.HasValue ? (TransactionType)request.Type.Value : null,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CategoryId = request.CategoryId,
            CurrencyId = request.CurrencyId,
            SortBy = request.SortBy,
            SortOrder = request.SortOrder,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return await _transactionRepository.GetTransactionsWithDetailsAsync(filters, cancellationToken);
    }
}
