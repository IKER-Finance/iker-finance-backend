using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Domain.Entities;
using IkerFinance.Infrastructure.Data;

namespace IkerFinance.Infrastructure.Services;

public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly ApplicationDbContext _context;

    public CurrencyConversionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> ConvertAsync(decimal amount, int fromCurrencyId, int toCurrencyId)
    {
        if (fromCurrencyId == toCurrencyId)
            return amount;

        var exchangeRate = await GetExchangeRateAsync(fromCurrencyId, toCurrencyId);
        return amount * exchangeRate.Rate;
    }

    public async Task<ExchangeRate> GetExchangeRateAsync(int fromCurrencyId, int toCurrencyId)
    {
        var rate = await _context.ExchangeRates
            .Where(r => r.FromCurrencyId == fromCurrencyId 
                     && r.ToCurrencyId == toCurrencyId 
                     && r.IsActive)
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync();

        if (rate == null)
            throw new NotFoundException($"Exchange rate not found from currency {fromCurrencyId} to {toCurrencyId}");

        return rate;
    }

    public async Task<bool> RatesExistAsync(int fromCurrencyId, int toCurrencyId)
    {
        return await _context.ExchangeRates
            .AnyAsync(r => r.FromCurrencyId == fromCurrencyId 
                        && r.ToCurrencyId == toCurrencyId 
                        && r.IsActive);
    }

    public async Task<List<Currency>> GetAvailableCurrenciesForAsync(int currencyId)
    {
        var currencyIds = await _context.ExchangeRates
            .Where(r => r.FromCurrencyId == currencyId && r.IsActive)
            .Select(r => r.ToCurrencyId)
            .Distinct()
            .ToListAsync();

        return await _context.Currencies
            .Where(c => currencyIds.Contains(c.Id) && c.IsActive)
            .ToListAsync();
    }
}