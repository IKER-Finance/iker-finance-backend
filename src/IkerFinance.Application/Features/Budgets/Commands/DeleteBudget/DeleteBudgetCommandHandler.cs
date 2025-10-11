using MediatR;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Domain.Entities;

namespace IkerFinance.Application.Features.Budgets.Commands.DeleteBudget;

public sealed class DeleteBudgetCommandHandler : IRequestHandler<DeleteBudgetCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IReadRepository<Budget> _budgetRepository;

    public DeleteBudgetCommandHandler(
        IApplicationDbContext context,
        IReadRepository<Budget> budgetRepository)
    {
        _context = context;
        _budgetRepository = budgetRepository;
    }

    public async Task<Unit> Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetAsync(
            b => b.Id == request.Id && b.UserId == request.UserId,
            cancellationToken);

        if (budget == null)
            throw new NotFoundException("Budget", request.Id);

        _context.Remove(budget);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}