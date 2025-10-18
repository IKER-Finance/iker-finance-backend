using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.ExchangeRates;
using IkerFinance.Infrastructure.Data;

namespace IkerFinance.Infrastructure.Repositories;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly ApplicationDbContext _context;

    public ExchangeRateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<ExchangeRateDto>> GetExchangeRatesAsync(
        ExchangeRateFilters filters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ExchangeRates
            .Include(er => er.FromCurrency)
            .Include(er => er.ToCurrency)
            .AsQueryable();

        // Search term filter - search in currency codes or names
        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var searchTerm = filters.SearchTerm.ToLower();
            query = query.Where(er =>
                er.FromCurrency.Code.ToLower().Contains(searchTerm) ||
                er.FromCurrency.Name.ToLower().Contains(searchTerm) ||
                er.ToCurrency.Code.ToLower().Contains(searchTerm) ||
                er.ToCurrency.Name.ToLower().Contains(searchTerm));
        }

        // Currency filters
        if (filters.FromCurrencyId.HasValue)
            query = query.Where(er => er.FromCurrencyId == filters.FromCurrencyId.Value);

        if (filters.ToCurrencyId.HasValue)
            query = query.Where(er => er.ToCurrencyId == filters.ToCurrencyId.Value);

        // Active status filter
        if (filters.IsActive.HasValue)
            query = query.Where(er => er.IsActive == filters.IsActive.Value);

        // Effective date range filters
        if (filters.EffectiveDateFrom.HasValue)
        {
            var effectiveDateFrom = DateTime.SpecifyKind(filters.EffectiveDateFrom.Value.Date, DateTimeKind.Utc);
            query = query.Where(er => er.EffectiveDate >= effectiveDateFrom);
        }

        if (filters.EffectiveDateTo.HasValue)
        {
            var effectiveDateTo = DateTime.SpecifyKind(filters.EffectiveDateTo.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(er => er.EffectiveDate <= effectiveDateTo);
        }

        // Sorting
        query = filters.SortBy.ToLower() switch
        {
            "effectivedate" => filters.SortOrder.ToLower() == "asc"
                ? query.OrderBy(er => er.EffectiveDate)
                : query.OrderByDescending(er => er.EffectiveDate),
            "rate" => filters.SortOrder.ToLower() == "asc"
                ? query.OrderBy(er => er.Rate)
                : query.OrderByDescending(er => er.Rate),
            "fromcurrency" => filters.SortOrder.ToLower() == "asc"
                ? query.OrderBy(er => er.FromCurrency.Code)
                : query.OrderByDescending(er => er.FromCurrency.Code),
            "tocurrency" => filters.SortOrder.ToLower() == "asc"
                ? query.OrderBy(er => er.ToCurrency.Code)
                : query.OrderByDescending(er => er.ToCurrency.Code),
            _ => filters.SortOrder.ToLower() == "asc"
                ? query.OrderBy(er => er.EffectiveDate)
                : query.OrderByDescending(er => er.EffectiveDate)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var exchangeRates = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .GroupJoin(
                _context.Users,
                er => er.UpdatedByUserId,
                u => u.Id,
                (er, users) => new { ExchangeRate = er, UpdatedBy = users.FirstOrDefault() }
            )
            .Select(x => new ExchangeRateDto
            {
                Id = x.ExchangeRate.Id,
                FromCurrencyId = x.ExchangeRate.FromCurrencyId,
                FromCurrencyCode = x.ExchangeRate.FromCurrency.Code,
                FromCurrencyName = x.ExchangeRate.FromCurrency.Name,
                ToCurrencyId = x.ExchangeRate.ToCurrencyId,
                ToCurrencyCode = x.ExchangeRate.ToCurrency.Code,
                ToCurrencyName = x.ExchangeRate.ToCurrency.Name,
                Rate = x.ExchangeRate.Rate,
                EffectiveDate = x.ExchangeRate.EffectiveDate,
                IsActive = x.ExchangeRate.IsActive,
                LastUpdated = x.ExchangeRate.LastUpdated,
                UpdatedByUserId = x.ExchangeRate.UpdatedByUserId,
                UpdatedByUserName = x.UpdatedBy != null ? x.UpdatedBy.FirstName + " " + x.UpdatedBy.LastName : null,
                CreatedAt = x.ExchangeRate.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<ExchangeRateDto>
        {
            Data = exchangeRates,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }
}
