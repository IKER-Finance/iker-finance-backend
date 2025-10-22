using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using TransactionEntity = IkerFinance.Domain.Entities.Transaction;

namespace IkerFinance.Domain.DomainServices.Transaction;

public class TransactionService
{
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
        ValidateParameters(userId, amount, description);

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
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        ValidateParameters(transaction.UserId, amount, description);

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
        TransactionEntity transaction,
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

    private void ValidateParameters(string userId, decimal amount, string description)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (amount <= 0)
            throw new ArgumentException("Transaction amount must be positive", nameof(amount));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Transaction description cannot be empty", nameof(description));
    }
}
