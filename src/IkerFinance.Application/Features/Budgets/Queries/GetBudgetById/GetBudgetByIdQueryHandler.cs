using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Shared.DTOs.Budgets;

namespace IkerFinance.Application.Features.Budgets.Queries.GetBudgetById;

public class GetBudgetByIdQueryHandler : IRequestHandler<GetBudgetByIdQuery, BudgetDto>
{
    private readonly IApplicationDbContext _context;

    public GetBudgetByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BudgetDto> Handle(GetBudgetByIdQuery request, CancellationToken cancellationToken)
    {
        var budget = await _context.Budgets
            .Include(b => b.Currency)
            .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
            .Where(b => b.Id == request.Id && b.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (budget == null)
            throw new NotFoundException("Budget", request.Id);

        return new BudgetDto
        {
            Id = budget.Id,
            Name = budget.Name,
            Period = budget.Period,
            StartDate = budget.StartDate,
            EndDate = budget.EndDate,
            Amount = budget.Amount,
            CurrencyId = budget.CurrencyId,
            CurrencyCode = budget.Currency.Code,
            CurrencySymbol = budget.Currency.Symbol,
            IsActive = budget.IsActive,
            Description = budget.Description,
            AllowOverlap = budget.AllowOverlap,
            AlertAt80Percent = budget.AlertAt80Percent,
            AlertAt100Percent = budget.AlertAt100Percent,
            AlertsEnabled = budget.AlertsEnabled,
            Categories = budget.Categories.Select(bc => new BudgetCategoryDto
            {
                CategoryId = bc.CategoryId,
                CategoryName = bc.Category.Name,
                Amount = bc.Amount
            }).ToList(),
            CreatedAt = budget.CreatedAt
        };
    }
}