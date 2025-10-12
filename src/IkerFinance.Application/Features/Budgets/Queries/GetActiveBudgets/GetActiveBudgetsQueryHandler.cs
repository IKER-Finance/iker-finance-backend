using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Application.Features.Budgets.Queries.GetActiveBudgets;

public sealed class GetActiveBudgetsQueryHandler : IRequestHandler<GetActiveBudgetsQuery, ActiveBudgetsSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyConversionService _conversionService;

    public GetActiveBudgetsQueryHandler(
        IApplicationDbContext context,
        ICurrencyConversionService conversionService)
    {
        _context = context;
        _conversionService = conversionService;
    }

    public async Task<ActiveBudgetsSummaryDto> Handle(GetActiveBudgetsQuery request, CancellationToken cancellationToken)
    {
        // Get user and home currency
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user?.HomeCurrencyId == null)
            throw new BadRequestException("User home currency not configured");

        var homeCurrency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == user.HomeCurrencyId.Value, cancellationToken);

        if (homeCurrency == null)
            throw new NotFoundException("Currency", user.HomeCurrencyId.Value);

        // Get all active budgets for user (IsActive = true and current date within date range)
        var currentDate = DateTime.UtcNow;
        var activeBudgets = await _context.Budgets
            .Include(b => b.Currency)
            .Include(b => b.Category)
            .Where(b => b.UserId == request.UserId
                && b.IsActive
                && b.StartDate <= currentDate
                && b.EndDate >= currentDate)
            .ToListAsync(cancellationToken);

        // Initialize counters
        int onTrackCount = 0;
        int warningCount = 0;
        int overBudgetCount = 0;
        decimal totalBudgetedInHome = 0;
        decimal totalSpentInHome = 0;

        var budgetItems = new List<ActiveBudgetItemDto>();

        // Process each budget
        foreach (var budget in activeBudgets)
        {
            // Get transactions for this budget category in budget period
            var transactions = await _context.Transactions
                .Include(t => t.ConvertedCurrency)
                .Where(t => t.UserId == request.UserId
                    && t.Type == TransactionType.Expense
                    && t.CategoryId == budget.CategoryId
                    && t.Date >= budget.StartDate
                    && t.Date <= budget.EndDate)
                .ToListAsync(cancellationToken);

            // Calculate total spent in budget currency
            decimal spentInBudgetCurrency = 0;
            foreach (var transaction in transactions)
            {
                var amountInBudgetCurrency = await _conversionService.ConvertAsync(
                    transaction.ConvertedAmount,
                    transaction.ConvertedCurrencyId,
                    budget.CurrencyId);

                spentInBudgetCurrency += amountInBudgetCurrency;
            }

            decimal remaining = budget.Amount - spentInBudgetCurrency;
            decimal percentage = budget.Amount > 0 ? (spentInBudgetCurrency / budget.Amount) * 100 : 0;
            string status = DetermineStatus(percentage);

            // Update counters
            if (status == "OnTrack") onTrackCount++;
            else if (status == "Warning") warningCount++;
            else if (status == "OverBudget") overBudgetCount++;

            // Convert to home currency for totals
            decimal budgetAmountInHome = await _conversionService.ConvertAsync(
                budget.Amount,
                budget.CurrencyId,
                user.HomeCurrencyId.Value);

            decimal spentAmountInHome = await _conversionService.ConvertAsync(
                spentInBudgetCurrency,
                budget.CurrencyId,
                user.HomeCurrencyId.Value);

            totalBudgetedInHome += budgetAmountInHome;
            totalSpentInHome += spentAmountInHome;

            // Build budget item
            var budgetItem = new ActiveBudgetItemDto
            {
                Id = budget.Id,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category.Name,
                CategoryIcon = budget.Category.Icon,
                CategoryColor = budget.Category.Color,
                Amount = budget.Amount,
                SpentAmount = Math.Round(spentInBudgetCurrency, 2),
                RemainingAmount = Math.Round(remaining, 2),
                PercentageSpent = Math.Round(percentage, 2),
                Status = status,
                CurrencyCode = budget.Currency.Code,
                CurrencySymbol = budget.Currency.Symbol,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate
            };

            budgetItems.Add(budgetItem);
        }

        // Build response
        return new ActiveBudgetsSummaryDto
        {
            TotalBudgets = activeBudgets.Count,
            TotalBudgetedAmount = Math.Round(totalBudgetedInHome, 2),
            TotalSpentAmount = Math.Round(totalSpentInHome, 2),
            BudgetsOnTrack = onTrackCount,
            BudgetsWarning = warningCount,
            BudgetsOverBudget = overBudgetCount,
            HomeCurrencyCode = homeCurrency.Code,
            HomeCurrencySymbol = homeCurrency.Symbol,
            Budgets = budgetItems.OrderByDescending(b => b.PercentageSpent).ToList()
        };
    }

    private static string DetermineStatus(decimal percentage)
    {
        if (percentage >= 100) return "OverBudget";
        if (percentage >= 80) return "Warning";
        return "OnTrack";
    }
}
