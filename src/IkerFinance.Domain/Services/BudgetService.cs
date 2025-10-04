using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Domain.Services;

public class BudgetService
{
    public Budget Create(
        string userId,
        string name,
        int currencyId,
        decimal amount,
        BudgetPeriod period,
        DateTime startDate,
        string? description)
    {
        var endDate = CalculateEndDate(startDate, period);

        return new Budget
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

    public void Update(
        Budget budget,
        string name,
        int currencyId,
        decimal amount,
        BudgetPeriod period,
        DateTime startDate,
        string? description,
        bool isActive)
    {
        var endDate = CalculateEndDate(startDate, period);

        budget.Name = name;
        budget.CurrencyId = currencyId;
        budget.Amount = amount;
        budget.Period = period;
        budget.StartDate = startDate;
        budget.EndDate = endDate;
        budget.Description = description;
        budget.IsActive = isActive;
        budget.UpdatedAt = DateTime.UtcNow;
    }

    private DateTime CalculateEndDate(DateTime startDate, BudgetPeriod period)
    {
        return period switch
        {
            BudgetPeriod.Daily => startDate.AddDays(1),
            BudgetPeriod.Weekly => startDate.AddDays(7),
            BudgetPeriod.Monthly => startDate.AddMonths(1),
            BudgetPeriod.Quarterly => startDate.AddMonths(3),
            BudgetPeriod.Yearly => startDate.AddYears(1),
            _ => throw new InvalidOperationException("Invalid budget period")
        };
    }
}