using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Extensions;
using IkerFinance.Domain.Entities;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.Budgets;

namespace IkerFinance.Application.Features.Budgets.Queries.GetBudgets;

public class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, PaginatedResponse<BudgetDto>>
{
    private readonly IApplicationDbContext _context;

    private readonly Dictionary<string, Expression<Func<Budget, object>>> _sortExpressions = new()
    {
        { "StartDate", b => b.StartDate },
        { "Name", b => b.Name },
        { "Amount", b => b.Amount },
        { "CreatedAt", b => b.CreatedAt }
    };

    public GetBudgetsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<BudgetDto>> Handle(
        GetBudgetsQuery request, 
        CancellationToken cancellationToken)
    {
        request.ValidatePagination();

        var query = _context.Budgets
            .Include(b => b.Currency)
            .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
            .Where(b => b.UserId == request.UserId);

        var searchTerm = request.GetNormalizedSearchTerm();
        if (searchTerm != null)
        {
            query = query.Where(b =>
                b.Name.ToLower().Contains(searchTerm) ||
                (b.Description != null && b.Description.ToLower().Contains(searchTerm)));
        }

        if (request.IsActive.HasValue)
            query = query.Where(b => b.IsActive == request.IsActive.Value);

        if (request.Period.HasValue)
            query = query.Where(b => b.Period == request.Period.Value);

        if (request.StartDate.HasValue)
            query = query.Where(b => b.StartDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(b => b.EndDate <= request.EndDate.Value);

        query = query.ApplySorting(request.SortBy, request.SortOrder, _sortExpressions);

        return await query.ToPaginatedListAsync(
            request.PageNumber,
            request.PageSize,
            b => new BudgetDto
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
            },
            cancellationToken);
    }
}