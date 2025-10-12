using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Application.Features.Budgets.Queries.PreviewBudgetImpact;

public sealed class PreviewBudgetImpactQueryHandler : IRequestHandler<PreviewBudgetImpactQuery, BudgetImpactPreviewDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyConversionService _conversionService;

    public PreviewBudgetImpactQueryHandler(
        IApplicationDbContext context,
        ICurrencyConversionService conversionService)
    {
        _context = context;
        _conversionService = conversionService;
    }

    public async Task<BudgetImpactPreviewDto> Handle(PreviewBudgetImpactQuery request, CancellationToken cancellationToken)
    {
        // Get user and home currency
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user?.HomeCurrencyId == null)
            throw new BadRequestException("User home currency not configured");

        // Find budgets that cover this transaction date and category
        var affectingBudgets = await _context.Budgets
            .Include(b => b.Currency)
            .Include(b => b.Category)
            .Where(b => b.UserId == request.UserId
                && b.CategoryId == request.CategoryId
                && b.StartDate <= request.TransactionDate
                && b.EndDate >= request.TransactionDate)
            .ToListAsync(cancellationToken);

        if (!affectingBudgets.Any())
        {
            // No budgets affected
            return new BudgetImpactPreviewDto
            {
                AffectedBudgets = new List<AffectedBudgetDto>(),
                HasWarnings = false,
                Warnings = new List<string>()
            };
        }

        var affectedBudgets = new List<AffectedBudgetDto>();
        var warnings = new List<string>();
        bool hasWarnings = false;

        foreach (var budget in affectingBudgets)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Currency)
                .Where(t => t.UserId == request.UserId
                    && t.Type == TransactionType.Expense
                    && t.CategoryId == budget.CategoryId
                    && t.Date >= budget.StartDate
                    && t.Date <= budget.EndDate)
                .ToListAsync(cancellationToken);

            decimal currentSpent = 0;
            foreach (var transaction in transactions)
            {
                decimal amountInBudgetCurrency;

                if (transaction.CurrencyId == budget.CurrencyId)
                {
                    amountInBudgetCurrency = transaction.Amount;
                }
                else
                {
                    amountInBudgetCurrency = await _conversionService.ConvertAsync(
                        transaction.ConvertedAmount,
                        transaction.ConvertedCurrencyId,
                        budget.CurrencyId);
                }

                currentSpent += amountInBudgetCurrency;
            }

            decimal newTransactionInBudgetCurrency;

            if (request.CurrencyId == budget.CurrencyId)
            {
                newTransactionInBudgetCurrency = request.Amount;
            }
            else
            {
                decimal transactionInHome = await _conversionService.ConvertAsync(
                    request.Amount,
                    request.CurrencyId,
                    user.HomeCurrencyId.Value);

                newTransactionInBudgetCurrency = await _conversionService.ConvertAsync(
                    transactionInHome,
                    user.HomeCurrencyId.Value,
                    budget.CurrencyId);
            }

            // Calculate after state
            decimal afterSpent = currentSpent + newTransactionInBudgetCurrency;
            decimal currentRemaining = budget.Amount - currentSpent;
            decimal afterRemaining = budget.Amount - afterSpent;
            decimal currentPercentage = budget.Amount > 0 ? (currentSpent / budget.Amount) * 100 : 0;
            decimal afterPercentage = budget.Amount > 0 ? (afterSpent / budget.Amount) * 100 : 0;

            string statusBefore = DetermineStatus(currentPercentage);
            string statusAfter = DetermineStatus(afterPercentage);

            bool willTriggerAlert = false;
            string? alertMessage = null;

            // Check for alerts
            if (statusBefore != "OverBudget" && statusAfter == "OverBudget")
            {
                willTriggerAlert = true;
                alertMessage = $"This transaction will cause the '{budget.Category.Name}' budget to exceed its limit!";
                warnings.Add(alertMessage);
                hasWarnings = true;
            }
            else if (statusBefore == "OnTrack" && statusAfter == "Warning")
            {
                willTriggerAlert = true;
                alertMessage = $"This transaction will push the '{budget.Category.Name}' budget over 80% spent.";
            }

            // Build affected budget DTO
            var affectedBudget = new AffectedBudgetDto
            {
                BudgetId = budget.Id,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category.Name,
                CategoryIcon = budget.Category.Icon,
                CategoryColor = budget.Category.Color,
                CurrentSpent = Math.Round(currentSpent, 2),
                CurrentRemaining = Math.Round(currentRemaining, 2),
                CurrentPercentage = Math.Round(currentPercentage, 2),
                StatusBefore = statusBefore,
                AfterSpent = Math.Round(afterSpent, 2),
                AfterRemaining = Math.Round(afterRemaining, 2),
                AfterPercentage = Math.Round(afterPercentage, 2),
                StatusAfter = statusAfter,
                WillTriggerAlert = willTriggerAlert,
                AlertMessage = alertMessage,
                CurrencyCode = budget.Currency.Code,
                CurrencySymbol = budget.Currency.Symbol
            };

            affectedBudgets.Add(affectedBudget);
        }

        // Build response
        return new BudgetImpactPreviewDto
        {
            AffectedBudgets = affectedBudgets,
            HasWarnings = hasWarnings,
            Warnings = warnings
        };
    }

    private static string DetermineStatus(decimal percentage)
    {
        if (percentage >= 100) return "OverBudget";
        if (percentage >= 80) return "Warning";
        return "OnTrack";
    }
}
