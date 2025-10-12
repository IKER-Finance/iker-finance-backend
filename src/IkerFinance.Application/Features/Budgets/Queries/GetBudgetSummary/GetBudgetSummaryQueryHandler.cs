using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Application.Features.Budgets.Queries.GetBudgetSummary;

public sealed class GetBudgetSummaryQueryHandler : IRequestHandler<GetBudgetSummaryQuery, BudgetSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyConversionService _conversionService;

    public GetBudgetSummaryQueryHandler(
        IApplicationDbContext context,
        ICurrencyConversionService conversionService)
    {
        _context = context;
        _conversionService = conversionService;
    }

    public async Task<BudgetSummaryDto> Handle(GetBudgetSummaryQuery request, CancellationToken cancellationToken)
    {
        // Fetch budget with includes
        var budget = await _context.Budgets
            .Include(b => b.Currency)
            .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == request.BudgetId && b.UserId == request.UserId, cancellationToken);

        if (budget == null)
            throw new NotFoundException("Budget", request.BudgetId);

        // Fetch all expense transactions in budget period
        var transactions = await _context.Transactions
            .Include(t => t.ConvertedCurrency)
            .Include(t => t.Category)
            .Where(t => t.UserId == request.UserId
                && t.Type == TransactionType.Expense
                && t.Date >= budget.StartDate
                && t.Date <= budget.EndDate)
            .ToListAsync(cancellationToken);

        // Calculate total spent by converting all transactions to budget currency
        decimal totalSpent = 0;
        foreach (var transaction in transactions)
        {
            // Transaction.ConvertedAmount is already in home currency
            // Convert from home currency to budget currency
            var amountInBudgetCurrency = await _conversionService.ConvertAsync(
                transaction.ConvertedAmount,
                transaction.ConvertedCurrencyId,
                budget.CurrencyId);

            totalSpent += amountInBudgetCurrency;
        }

        // Calculate basic metrics
        decimal remaining = budget.Amount - totalSpent;
        decimal percentageSpent = budget.Amount > 0 ? (totalSpent / budget.Amount) * 100 : 0;
        string status = DetermineStatus(percentageSpent);

        // Calculate category-level spending
        var categorySummaries = new List<BudgetCategorySummaryDto>();
        foreach (var budgetCategory in budget.Categories)
        {
            decimal categorySpent = 0;

            // Get transactions for this category
            var categoryTransactions = transactions.Where(t => t.CategoryId == budgetCategory.CategoryId);

            foreach (var transaction in categoryTransactions)
            {
                var amountInBudgetCurrency = await _conversionService.ConvertAsync(
                    transaction.ConvertedAmount,
                    transaction.ConvertedCurrencyId,
                    budget.CurrencyId);

                categorySpent += amountInBudgetCurrency;
            }

            decimal categoryRemaining = budgetCategory.Amount - categorySpent;
            decimal categoryPercentage = budgetCategory.Amount > 0
                ? (categorySpent / budgetCategory.Amount) * 100
                : 0;

            categorySummaries.Add(new BudgetCategorySummaryDto
            {
                CategoryId = budgetCategory.CategoryId,
                CategoryName = budgetCategory.Category.Name,
                AllocatedAmount = budgetCategory.Amount,
                SpentAmount = Math.Round(categorySpent, 2),
                RemainingAmount = Math.Round(categoryRemaining, 2),
                PercentageSpent = Math.Round(categoryPercentage, 2),
                Status = DetermineStatus(categoryPercentage)
            });
        }

        // Build response
        return new BudgetSummaryDto
        {
            BudgetId = budget.Id,
            Name = budget.Name,
            Amount = budget.Amount,
            CurrencyCode = budget.Currency.Code,
            CurrencySymbol = budget.Currency.Symbol,
            SpentAmount = Math.Round(totalSpent, 2),
            RemainingAmount = Math.Round(remaining, 2),
            PercentageSpent = Math.Round(percentageSpent, 2),
            Status = status,
            IsActive = budget.IsActive,
            StartDate = budget.StartDate,
            EndDate = budget.EndDate,
            Description = budget.Description,
            Categories = categorySummaries,
            TransactionCount = transactions.Count,
            AlertAt80Percent = percentageSpent >= 80 && percentageSpent < 100,
            AlertAt100Percent = percentageSpent >= 100
        };
    }

    private static string DetermineStatus(decimal percentage)
    {
        if (percentage >= 100) return "OverBudget";
        if (percentage >= 80) return "Warning";
        return "OnTrack";
    }
}
