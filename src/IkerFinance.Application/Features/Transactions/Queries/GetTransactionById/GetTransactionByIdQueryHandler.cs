using MediatR;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.DTOs.Transactions;

namespace IkerFinance.Application.Features.Transactions.Queries.GetTransactionById;

public sealed class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionByIdQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetTransactionWithDetailsAsync(
            request.Id,
            request.UserId,
            cancellationToken);

        if (transaction == null)
            throw new NotFoundException("Transaction", request.Id);

        return transaction;
    }
}
