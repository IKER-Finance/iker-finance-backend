using IkerFinance.Domain.Enums;
using BudgetEntity = IkerFinance.Domain.Entities.Budget;

namespace IkerFinance.Domain.DomainServices.Budget;

/// <summary>
/// Domain service responsible for creating Budget entities.
/// Applies business rules and calculates initial values.
/// </summary>
public class BudgetFactory
{
    /// <summary>
    /// Creates a new Budget with calculated end date and default alert settings.
    /// </summary>
    public BudgetEntity Create(
        string userId,
        string name,
        int currencyId,
        decimal amount,
        BudgetPeriod period,
        DateTime startDate,
        string? description)
    {
        ValidateCreationParameters(userId, name, amount);

        var endDate = CalculateEndDate(startDate, period);

        return new BudgetEntity
        {
            UserId = userId,
            Name = name,
            CurrencyId = currencyId,
            Amount = amount,
            Period = period,
            StartDate = startDate,
            EndDate = endDate,
            Description = description,
            IsActive = true,
            AllowOverlap = false,
            AlertAt80Percent = 0.8m,
            AlertAt100Percent = 1.0m,
            AlertsEnabled = true
        };
    }

    /// <summary>
    /// Calculates the end date for a budget based on its period.
    /// </summary>
    public DateTime CalculateEndDate(DateTime startDate, BudgetPeriod period)
    {
        return period switch
        {
            BudgetPeriod.Daily => startDate.AddDays(1).AddSeconds(-1),
            BudgetPeriod.Weekly => startDate.AddDays(7).AddSeconds(-1),
            BudgetPeriod.Monthly => startDate.AddMonths(1).AddSeconds(-1),
            BudgetPeriod.Quarterly => startDate.AddMonths(3).AddSeconds(-1),
            BudgetPeriod.Yearly => startDate.AddYears(1).AddSeconds(-1),
            _ => throw new InvalidOperationException($"Invalid budget period: {period}")
        };
    }

    private void ValidateCreationParameters(string userId, string name, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Budget name cannot be empty", nameof(name));
        
        if (amount <= 0)
            throw new ArgumentException("Budget amount must be positive", nameof(amount));
    }
}