using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Budgets;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Infrastructure.Data;

namespace IkerFinance.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly ApplicationDbContext _context;

    public BudgetRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<BudgetDto>> GetBudgetsWithDetailsAsync(
        BudgetFilters filters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Budgets
            .Include(b => b.Currency)
            .Include(b => b.Category)
            .Where(b => b.UserId == filters.UserId);

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var searchTerm = filters.SearchTerm.ToLower();
            query = query.Where(b =>
                b.Category.Name.ToLower().Contains(searchTerm) ||
                (b.Description != null && b.Description.ToLower().Contains(searchTerm)));
        }

        if (filters.IsActive.HasValue)
            query = query.Where(b => b.IsActive == filters.IsActive.Value);

        if (filters.Period.HasValue)
            query = query.Where(b => b.Period == filters.Period.Value);

        if (filters.StartDate.HasValue)
            query = query.Where(b => b.StartDate >= filters.StartDate.Value);

        if (filters.EndDate.HasValue)
            query = query.Where(b => b.EndDate <= filters.EndDate.Value);

        query = filters.SortOrder.ToLower() == "asc"
            ? query.OrderBy(b => b.StartDate)
            : query.OrderByDescending(b => b.StartDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var budgets = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(b => new BudgetDto
            {
                Id = b.Id,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.Name,
                CategoryIcon = b.Category.Icon,
                CategoryColor = b.Category.Color,
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
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<BudgetDto>
        {
            Data = budgets,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    public async Task<BudgetDto?> GetBudgetWithDetailsAsync(
        int id,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Budgets
            .Include(b => b.Currency)
            .Include(b => b.Category)
            .Where(b => b.Id == id && b.UserId == userId)
            .Select(b => new BudgetDto
            {
                Id = b.Id,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.Name,
                CategoryIcon = b.Category.Icon,
                CategoryColor = b.Category.Color,
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
                CreatedAt = b.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
