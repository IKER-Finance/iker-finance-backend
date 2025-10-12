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
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == request.BudgetId && b.UserId == request.UserId, cancellationToken);

        if (budget == null)
            throw new NotFoundException("Budget", request.BudgetId);

        var transactions = await _context.Transactions
            .Include(t => t.Currency)
            .Include(t => t.ConvertedCurrency)
            .Where(t => t.UserId == request.UserId
                && t.Type == TransactionType.Expense
                && t.CategoryId == budget.CategoryId
                && t.Date >= budget.StartDate
                && t.Date <= budget.EndDate)
            .ToListAsync(cancellationToken);

        decimal totalSpent = 0;
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

            totalSpent += amountInBudgetCurrency;
        }

        // Calculate basic metrics
        decimal remaining = budget.Amount - totalSpent;
        decimal percentageSpent = budget.Amount > 0 ? (totalSpent / budget.Amount) * 100 : 0;
        string status = DetermineStatus(percentageSpent);

        // Build response
        return new BudgetSummaryDto
        {
            BudgetId = budget.Id,
            CategoryId = budget.CategoryId,
            CategoryName = budget.Category.Name,
            CategoryIcon = budget.Category.Icon,
            CategoryColor = budget.Category.Color,
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
