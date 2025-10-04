using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Domain.Services;

public class TransactionService
{
    public Transaction Create(
        string userId,
        decimal amount,
        int currencyId,
        int homeCurrencyId,
        int categoryId,
        TransactionType type,
        string description,
        string? notes,
        DateTime date,
        ExchangeRate? exchangeRate)
    {
        var transaction = new Transaction
        {
            UserId = userId,
            Amount = amount,
            CurrencyId = currencyId,
            ConvertedCurrencyId = homeCurrencyId,
            Type = type,
            Description = description,
            Notes = notes,
            Date = date,
            CategoryId = categoryId
        };

        ApplyCurrencyConversion(transaction, homeCurrencyId, exchangeRate);
        
        return transaction;
    }

    public void Update(
        Transaction transaction,
        decimal amount,
        int currencyId,
        int homeCurrencyId,
        int categoryId,
        TransactionType type,
        string description,
        string? notes,
        DateTime date,
        ExchangeRate? exchangeRate)
    {
        transaction.Amount = amount;
        transaction.CurrencyId = currencyId;
        transaction.Type = type;
        transaction.Description = description;
        transaction.Notes = notes;
        transaction.Date = date;
        transaction.CategoryId = categoryId;

        ApplyCurrencyConversion(transaction, homeCurrencyId, exchangeRate);
    }

    private void ApplyCurrencyConversion(
        Transaction transaction, 
        int homeCurrencyId, 
        ExchangeRate? exchangeRate)
    {
        if (transaction.CurrencyId == homeCurrencyId)
        {
            transaction.ConvertedAmount = transaction.Amount;
            transaction.ExchangeRate = 1.0m;
            transaction.ExchangeRateDate = DateTime.UtcNow;
            return;
        }

        if (exchangeRate == null)
        {
            throw new InvalidOperationException(
                "Exchange rate is required for cross-currency transactions");
        }

        if (!exchangeRate.IsCurrentlyValid())
        {
            throw new InvalidOperationException(
                "Exchange rate is not currently valid");
        }

        transaction.ExchangeRate = exchangeRate.Rate;
        transaction.ConvertedAmount = transaction.Amount * exchangeRate.Rate;
        transaction.ExchangeRateDate = DateTime.UtcNow;
    }
}