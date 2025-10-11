using IkerFinance.Domain.Enums;
using BudgetEntity = IkerFinance.Domain.Entities.Budget;

namespace IkerFinance.Domain.DomainServices.Budget;

/// <summary>
/// Domain service responsible for updating Budget entities.
/// Ensures business rules are maintained during updates.
/// </summary>
public class BudgetUpdater
{
    private readonly BudgetFactory _budgetFactory;

    public BudgetUpdater(BudgetFactory budgetFactory)
    {
        _budgetFactory = budgetFactory;
    }

    /// <summary>
    /// Updates an existing budget with new values.
    /// Recalculates end date based on new period and start date.
    /// </summary>
    public void Update(
        BudgetEntity budget,
        string name,
        int currencyId,
        decimal amount,
        BudgetPeriod period,
        DateTime startDate,
        string? description,
        bool isActive)
    {
        ValidateUpdateParameters(budget, name, amount);

        var endDate = _budgetFactory.CalculateEndDate(startDate, period);

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

    /// <summary>
    /// Updates only the budget amount, useful for quick adjustments.
    /// </summary>
    public void UpdateAmount(BudgetEntity budget, decimal newAmount)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));
        
        if (newAmount <= 0)
            throw new ArgumentException("Budget amount must be positive", nameof(newAmount));

        budget.Amount = newAmount;
        budget.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates or deactivates a budget.
    /// </summary>
    public void SetActiveStatus(BudgetEntity budget, bool isActive)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));

        budget.IsActive = isActive;
        budget.UpdatedAt = DateTime.UtcNow;
    }

    private void ValidateUpdateParameters(BudgetEntity budget, string name, decimal amount)
    {
        if (budget == null)
            throw new ArgumentNullException(nameof(budget));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Budget name cannot be empty", nameof(name));
        
        if (amount <= 0)
            throw new ArgumentException("Budget amount must be positive", nameof(amount));
    }
}