using MediatR;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Domain.Entities;

namespace IkerFinance.Application.Features.Transactions.Commands.DeleteTransaction;

public sealed class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IReadRepository<Transaction> _transactionRepository;

    public DeleteTransactionCommandHandler(
        IApplicationDbContext context,
        IReadRepository<Transaction> transactionRepository)
    {
        _context = context;
        _transactionRepository = transactionRepository;
    }

    public async Task<Unit> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetAsync(
            t => t.Id == request.Id && t.UserId == request.UserId,
            cancellationToken);

        if (transaction == null)
            throw new NotFoundException("Transaction", request.Id);

        _context.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
