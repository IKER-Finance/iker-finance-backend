using IkerFinance.Domain.Enums;
using BudgetEntity = IkerFinance.Domain.Entities.Budget;
using TransactionEntity = IkerFinance.Domain.Entities.Transaction;

namespace IkerFinance.Domain.DomainServices.Budget;

public class BudgetCalculator
{
    public decimal CalculateRemaining(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));
        
        if (transactions == null)
            throw new ArgumentNullException(nameof(transactions));

        var spent = CalculateTotalSpent(transactions);
        return budget.Amount - spent;
    }

    public decimal CalculateTotalSpent(IEnumerable<TransactionEntity> transactions)
    {
        if (transactions == null)
            throw new ArgumentNullException(nameof(transactions));

        return transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.ConvertedAmount);
    }

    public decimal CalculateUsagePercentage(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));
        
        if (budget.Amount == 0)
            return 0;

        var spent = CalculateTotalSpent(transactions);
        return (spent / budget.Amount) * 100;
    }

    public bool IsOverBudget(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        return CalculateRemaining(budget, transactions) < 0;
    }

    public bool IsApproachingLimit(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        if (budget.AlertAt80Percent == 0)
            return false;

        var usagePercentage = CalculateUsagePercentage(budget, transactions);
        return usagePercentage >= (budget.AlertAt80Percent * 100);
    }

    public bool HasReachedLimit(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        if (budget.AlertAt100Percent == 0)
            return false;

        var usagePercentage = CalculateUsagePercentage(budget, transactions);
        return usagePercentage >= (budget.AlertAt100Percent * 100);
    }

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

    public DateTime? ProjectExhaustionDate(BudgetEntity budget, IEnumerable<TransactionEntity> transactions)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));

        var remaining = CalculateRemaining(budget, transactions);

        if (remaining <= 0)
            return DateTime.UtcNow;

        var averageDaily = CalculateAverageDailySpending(budget, transactions);

        if (averageDaily <= 0)
            return null;

        var daysRemaining = (int)(remaining / averageDaily);
        var projectedDate = DateTime.UtcNow.AddDays(daysRemaining);

        return projectedDate > budget.EndDate ? budget.EndDate : projectedDate;
    }

    public int CalculateRemainingDays(BudgetEntity budget)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));

        var currentDate = DateTime.UtcNow;
        
        if (currentDate > budget.EndDate)
            return 0;

        return (budget.EndDate - currentDate).Days;
    }

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