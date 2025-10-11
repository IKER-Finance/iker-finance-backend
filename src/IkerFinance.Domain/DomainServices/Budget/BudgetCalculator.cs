using IkerFinance.Domain.Enums;
using BudgetEntity = IkerFinance.Domain.Entities.Budget;
using TransactionEntity = IkerFinance.Domain.Entities.Transaction;

namespace IkerFinance.Domain.DomainServices.Budget;

/// <summary>
/// Domain service responsible for budget calculations and analysis.
/// Contains pure calculation logic with no side effects.
/// </summary>
public class BudgetCalculator
{
    /// <summary>
    /// Calculates the remaining budget amount after expenses.
    /// </summary>
    public decimal CalculateRemaining(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));
        
        if (transactions == null)
            throw new ArgumentNullException(nameof(transactions));

        var spent = CalculateTotalSpent(transactions);
        return budget.Amount - spent;
    }

    /// <summary>
    /// Calculates total amount spent from transactions.
    /// </summary>
    public decimal CalculateTotalSpent(IEnumerable<TransactionEntity> transactions)
    {
        if (transactions == null)
            throw new ArgumentNullException(nameof(transactions));

        return transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.ConvertedAmount);
    }

    /// <summary>
    /// Calculates the percentage of budget used.
    /// </summary>
    public decimal CalculateUsagePercentage(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));
        
        if (budget.Amount == 0)
            return 0;

        var spent = CalculateTotalSpent(transactions);
        return (spent / budget.Amount) * 100;
    }

    /// <summary>
    /// Checks if the budget has been exceeded.
    /// </summary>
    public bool IsOverBudget(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        return CalculateRemaining(budget, transactions) < 0;
    }

    /// <summary>
    /// Checks if the budget is approaching its limit based on alert threshold.
    /// </summary>
    public bool IsApproachingLimit(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        if (budget.AlertAt80Percent == 0)
            return false;

        var usagePercentage = CalculateUsagePercentage(budget, transactions);
        return usagePercentage >= (budget.AlertAt80Percent * 100);
    }

    /// <summary>
    /// Checks if the budget has reached the 100% alert threshold.
    /// </summary>
    public bool HasReachedLimit(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        if (budget.AlertAt100Percent == 0)
            return false;

        var usagePercentage = CalculateUsagePercentage(budget, transactions);
        return usagePercentage >= (budget.AlertAt100Percent * 100);
    }

    /// <summary>
    /// Calculates the average daily spending rate.
    /// </summary>
    public decimal CalculateAverageDailySpending(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));

        var totalSpent = CalculateTotalSpent(transactions);
        var daysPassed = (DateTime.UtcNow - budget.StartDate).Days;

        if (daysPassed <= 0)
            return 0;

        return totalSpent / daysPassed;
    }

    /// <summary>
    /// Projects when the budget will be exhausted based on current spending rate.
    /// Returns null if no spending yet or budget won't be exhausted.
    /// </summary>
    public DateTime? ProjectExhaustionDate(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));

        var remaining = CalculateRemaining(budget, transactions);
        
        if (remaining <= 0)
            return DateTime.UtcNow; // Already exhausted

        var averageDaily = CalculateAverageDailySpending(budget, transactions);
        
        if (averageDaily <= 0)
            return null; // No spending yet

        var daysRemaining = (int)(remaining / averageDaily);
        var projectedDate = DateTime.UtcNow.AddDays(daysRemaining);

        // Don't project beyond budget end date
        return projectedDate > budget.EndDate ? budget.EndDate : projectedDate;
    }

    /// <summary>
    /// Calculates remaining days in the budget period.
    /// </summary>
    public int CalculateRemainingDays(BudgetEntity budget)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));

        var currentDate = DateTime.UtcNow;
        
        if (currentDate > budget.EndDate)
            return 0;

        return (budget.EndDate - currentDate).Days;
    }

    /// <summary>
    /// Checks if a budget is currently active (within date range and enabled).
    /// </summary>
    public bool IsBudgetActive(BudgetEntity budget)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));

        var currentDate = DateTime.UtcNow;
        
        return budget.IsActive && 
               currentDate >= budget.StartDate && 
               currentDate <= budget.EndDate;
    }
}