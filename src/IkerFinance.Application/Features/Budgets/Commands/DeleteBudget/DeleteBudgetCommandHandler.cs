using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;

namespace IkerFinance.Application.Features.Budgets.Commands.DeleteBudget;

public sealed class DeleteBudgetCommandHandler : IRequestHandler<DeleteBudgetCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteBudgetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.Id == request.Id && b.UserId == request.UserId, cancellationToken);

        if (budget == null)
            throw new NotFoundException("Budget", request.Id);

        _context.Remove(budget);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}