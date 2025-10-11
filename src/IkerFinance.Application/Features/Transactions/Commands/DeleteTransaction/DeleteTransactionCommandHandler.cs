using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;

namespace IkerFinance.Application.Features.Transactions.Commands.DeleteTransaction;

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .Where(t => t.Id == request.Id && t.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (transaction == null)
            throw new NotFoundException("Transaction", request.Id);

        _context.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}