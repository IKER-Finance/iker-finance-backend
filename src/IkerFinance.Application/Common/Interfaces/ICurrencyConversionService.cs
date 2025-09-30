using IkerFinance.Domain.Entities;

namespace IkerFinance.Application.Common.Interfaces;

public interface ICurrencyConversionService
{
    Task<decimal> ConvertAsync(decimal amount, int fromCurrencyId, int toCurrencyId);
    Task<ExchangeRate> GetExchangeRateAsync(int fromCurrencyId, int toCurrencyId);
    Task<bool> RatesExistAsync(int fromCurrencyId, int toCurrencyId);
    Task<List<Currency>> GetAvailableCurrenciesForAsync(int currencyId);
}