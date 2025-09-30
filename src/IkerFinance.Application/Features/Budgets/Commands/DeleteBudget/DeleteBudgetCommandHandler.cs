using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;

namespace IkerFinance.Application.Features.Budgets.Commands.DeleteBudget;

public class DeleteBudgetCommandHandler : IRequestHandler<DeleteBudgetCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteBudgetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _context.Budgets
            .Include(b => b.Categories)
            .Where(b => b.Id == request.Id && b.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (budget == null)
            throw new NotFoundException("Budget", request.Id);

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}