using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using TransactionEntity = IkerFinance.Domain.Entities.Transaction;

namespace IkerFinance.Domain.DomainServices.Transaction;

/// <summary>
/// Domain service responsible for updating Transaction entities.
/// Recalculates currency conversion when updates occur.
/// </summary>
public class TransactionUpdater
{
    /// <summary>
    /// Updates an existing transaction with new values and recalculates currency conversion.
    /// </summary>
    public void Update(
        TransactionEntity transaction,
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
        ValidateUpdateParameters(transaction, amount, description);

        transaction.Amount = amount;
        transaction.CurrencyId = currencyId;
        transaction.Type = type;
        transaction.Description = description;
        transaction.Notes = notes;
        transaction.Date = date;
        transaction.CategoryId = categoryId;

        ApplyCurrencyConversion(transaction, homeCurrencyId, exchangeRate);
    }

    /// <summary>
    /// Updates only the transaction amount and recalculates conversion.
    /// </summary>
    public void UpdateAmount(TransactionEntity transaction, decimal newAmount, ExchangeRate? exchangeRate, int homeCurrencyId)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));
        
        if (newAmount <= 0)
            throw new ArgumentException("Transaction amount must be positive", nameof(newAmount));

        transaction.Amount = newAmount;
        
        ApplyCurrencyConversion(transaction, homeCurrencyId, exchangeRate);
    }

    /// <summary>
    /// Updates the transaction category.
    /// </summary>
    public void UpdateCategory(TransactionEntity transaction, int newCategoryId)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        transaction.CategoryId = newCategoryId;
    }

    /// <summary>
    /// Applies currency conversion to a transaction.
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

    private void ValidateUpdateParameters(TransactionEntity transaction, decimal amount, string description)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));
        
        if (amount <= 0)
            throw new ArgumentException("Transaction amount must be positive", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Transaction description cannot be empty", nameof(description));
    }
}