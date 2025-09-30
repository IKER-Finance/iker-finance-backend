using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Shared.DTOs.Budgets;

namespace IkerFinance.Application.Features.Budgets.Queries.GetBudgets;

public class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, List<BudgetDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBudgetsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BudgetDto>> Handle(GetBudgetsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Budgets
            .Include(b => b.Currency)
            .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
            .Where(b => b.UserId == request.UserId);

        if (request.IsActive.HasValue)
            query = query.Where(b => b.IsActive == request.IsActive.Value);

        if (request.Period.HasValue)
            query = query.Where(b => b.Period == request.Period.Value);

        if (request.StartDate.HasValue)
            query = query.Where(b => b.StartDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(b => b.EndDate <= request.EndDate.Value);

        var budgets = await query
            .OrderByDescending(b => b.StartDate)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return budgets.Select(b => new BudgetDto
        {
            Id = b.Id,
            Name = b.Name,
            Period = b.Period,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            Amount = b.Amount,
            CurrencyId = b.CurrencyId,
            CurrencyCode = b.Currency.Code,
            CurrencySymbol = b.Currency.Symbol,
            IsActive = b.IsActive,
            Description = b.Description,
            AllowOverlap = b.AllowOverlap,
            AlertAt80Percent = b.AlertAt80Percent,
            AlertAt100Percent = b.AlertAt100Percent,
            AlertsEnabled = b.AlertsEnabled,
            Categories = b.Categories.Select(bc => new BudgetCategoryDto
            {
                CategoryId = bc.CategoryId,
                CategoryName = bc.Category.Name,
                Amount = bc.Amount
            }).ToList(),
            CreatedAt = b.CreatedAt
        }).ToList();
    }
}