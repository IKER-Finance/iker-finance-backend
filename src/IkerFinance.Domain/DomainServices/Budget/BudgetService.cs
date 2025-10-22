using IkerFinance.Domain.Enums;
using BudgetEntity = IkerFinance.Domain.Entities.Budget;

namespace IkerFinance.Domain.DomainServices.Budget;

public class BudgetService
{
    public BudgetEntity Create(
        string userId,
        int categoryId,
        int currencyId,
        decimal amount,
        BudgetPeriod period,
        DateTime startDate,
        string? description)
    {
        ValidateParameters(userId, categoryId, amount);

        var endDate = CalculateEndDate(startDate, period);

        return new BudgetEntity
        {
            UserId = userId,
            CategoryId = categoryId,
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

    public void Update(
        BudgetEntity budget,
        int categoryId,
        int currencyId,
        decimal amount,
        BudgetPeriod period,
        DateTime startDate,
        string? description,
        bool isActive)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));

        ValidateParameters(budget.UserId, categoryId, amount);

        var endDate = CalculateEndDate(startDate, period);

        budget.CategoryId = categoryId;
        budget.CurrencyId = currencyId;
        budget.Amount = amount;
        budget.Period = period;
        budget.StartDate = startDate;
        budget.EndDate = endDate;
        budget.Description = description;
        budget.IsActive = isActive;
        budget.UpdatedAt = DateTime.UtcNow;
    }

    public DateTime CalculateEndDate(DateTime startDate, BudgetPeriod period)
    {
        return period switch
        {
            BudgetPeriod.Weekly => startDate.AddDays(7).AddSeconds(-1),
            BudgetPeriod.Monthly => startDate.AddMonths(1).AddSeconds(-1),
            BudgetPeriod.Quarterly => startDate.AddMonths(3).AddSeconds(-1),
            BudgetPeriod.Yearly => startDate.AddYears(1).AddSeconds(-1),
            _ => throw new InvalidOperationException($"Invalid budget period: {period}")
        };
    }

    private void ValidateParameters(string userId, int categoryId, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (categoryId <= 0)
            throw new ArgumentException("Category ID must be valid", nameof(categoryId));

        if (amount <= 0)
            throw new ArgumentException("Budget amount must be positive", nameof(amount));
    }
}
