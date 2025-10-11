using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.Transactions;
using IkerFinance.Infrastructure.Data;

namespace IkerFinance.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<TransactionDto>> GetTransactionsWithDetailsAsync(
        TransactionFilters filters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Include(t => t.Currency)
            .Include(t => t.ConvertedCurrency)
            .Include(t => t.Category)
            .Where(t => t.UserId == filters.UserId);

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var searchTerm = filters.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Description.ToLower().Contains(searchTerm) ||
                (t.Notes != null && t.Notes.ToLower().Contains(searchTerm)));
        }

        if (filters.Type.HasValue)
            query = query.Where(t => t.Type == filters.Type.Value);

        if (filters.StartDate.HasValue)
        {
            var startDateUtc = DateTime.SpecifyKind(filters.StartDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(t => t.Date >= startDateUtc);
        }

        if (filters.EndDate.HasValue)
        {
            var endDateUtc = DateTime.SpecifyKind(filters.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(t => t.Date <= endDateUtc);
        }

        if (filters.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filters.CategoryId.Value);

        if (filters.CurrencyId.HasValue)
            query = query.Where(t => t.CurrencyId == filters.CurrencyId.Value);

        query = filters.SortOrder.ToLower() == "asc"
            ? query.OrderBy(t => t.Date)
            : query.OrderByDescending(t => t.Date);

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                CurrencyId = t.CurrencyId,
                CurrencyCode = t.Currency.Code,
                CurrencySymbol = t.Currency.Symbol,
                ConvertedAmount = t.ConvertedAmount,
                ConvertedCurrencyId = t.ConvertedCurrencyId,
                ConvertedCurrencyCode = t.ConvertedCurrency.Code,
                ExchangeRate = t.ExchangeRate,
                Type = t.Type,
                Description = t.Description,
                Notes = t.Notes,
                Date = t.Date,
                CategoryId = t.CategoryId,
                CategoryName = t.Category.Name,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<TransactionDto>
        {
            Data = transactions,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    public async Task<TransactionDto?> GetTransactionWithDetailsAsync(
        int id,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Currency)
            .Include(t => t.ConvertedCurrency)
            .Include(t => t.Category)
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                CurrencyId = t.CurrencyId,
                CurrencyCode = t.Currency.Code,
                CurrencySymbol = t.Currency.Symbol,
                ConvertedAmount = t.ConvertedAmount,
                ConvertedCurrencyId = t.ConvertedCurrencyId,
                ConvertedCurrencyCode = t.ConvertedCurrency.Code,
                ExchangeRate = t.ExchangeRate,
                Type = t.Type,
                Description = t.Description,
                Notes = t.Notes,
                Date = t.Date,
                CategoryId = t.CategoryId,
                CategoryName = t.Category.Name,
                CreatedAt = t.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
