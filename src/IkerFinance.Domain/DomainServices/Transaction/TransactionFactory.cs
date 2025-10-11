using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using TransactionEntity = IkerFinance.Domain.Entities.Transaction;

namespace IkerFinance.Domain.DomainServices.Transaction;

/// <summary>
/// Domain service responsible for creating Transaction entities.
/// Handles currency conversion during creation.
/// </summary>
public class TransactionFactory
{
    /// <summary>
    /// Creates a new Transaction with currency conversion applied.
    /// </summary>
    public TransactionEntity Create(
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
        ValidateCreationParameters(userId, amount, description);

        var transaction = new TransactionEntity
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

    /// <summary>
    /// Applies currency conversion to a transaction.
    /// If same currency, sets 1:1 conversion. Otherwise uses provided exchange rate.
    /// </summary>
    private void ApplyCurrencyConversion(
        TransactionEntity transaction, 
        int homeCurrencyId, 
        ExchangeRate? exchangeRate)
    {
        // Same currency - no conversion needed
        if (transaction.CurrencyId == homeCurrencyId)
        {
            transaction.ConvertedAmount = transaction.Amount;
            transaction.ExchangeRate = 1.0m;
            transaction.ExchangeRateDate = DateTime.UtcNow;
            return;
        }

        // Cross-currency transaction - exchange rate required
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

    /// <summary>
    /// Calculates the converted amount for a given amount and exchange rate.
    /// </summary>
    public decimal CalculateConvertedAmount(decimal amount, decimal exchangeRate)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        if (exchangeRate <= 0)
            throw new ArgumentException("Exchange rate must be positive", nameof(exchangeRate));

        return amount * exchangeRate;
    }

    /// <summary>
    /// Determines if currency conversion is needed for a transaction.
    /// </summary>
    public bool RequiresCurrencyConversion(int transactionCurrencyId, int homeCurrencyId)
    {
        return transactionCurrencyId != homeCurrencyId;
    }

    private void ValidateCreationParameters(string userId, decimal amount, string description)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        
        if (amount <= 0)
            throw new ArgumentException("Transaction amount must be positive", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Transaction description cannot be empty", nameof(description));
    }
}