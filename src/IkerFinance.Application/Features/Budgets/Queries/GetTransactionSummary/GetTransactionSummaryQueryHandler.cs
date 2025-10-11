using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Domain.Enums;
using IkerFinance.Application.DTOs.Transactions;

namespace IkerFinance.Application.Features.Transactions.Queries.GetTransactionSummary;

public sealed class GetTransactionSummaryQueryHandler : IRequestHandler<GetTransactionSummaryQuery, TransactionSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionSummaryDto> Handle(
        GetTransactionSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user?.HomeCurrencyId == null)
            throw new NotFoundException("User", request.UserId);

        var homeCurrency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == user.HomeCurrencyId.Value, cancellationToken);

        var transactionsQuery = _context.Transactions
            .Where(t => t.UserId == request.UserId);

        if (request.StartDate.HasValue)
        {
            var startDateUtc = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
            transactionsQuery = transactionsQuery.Where(t => t.Date >= startDateUtc);
        }

        if (request.EndDate.HasValue)
        {
            var endDateUtc = DateTime.SpecifyKind(request.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            transactionsQuery = transactionsQuery.Where(t => t.Date <= endDateUtc);
        }

        var transactionList = await transactionsQuery.ToListAsync(cancellationToken);

        var categoryIds = transactionList.Select(t => t.CategoryId).Distinct().ToList();
        var categories = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var totalIncome = transactionList
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.ConvertedAmount);

        var totalExpenses = transactionList
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.ConvertedAmount);

        var topIncomeCategories = transactionList
            .Where(t => t.Type == TransactionType.Income)
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var category = categories.First(c => c.Id == g.Key);
                return new CategorySummary
                {
                    CategoryId = g.Key,
                    CategoryName = category.Name,
                    CategoryColor = category.Color,
                    CategoryIcon = category.Icon,
                    TotalAmount = g.Sum(t => t.ConvertedAmount),
                    TransactionCount = g.Count(),
                    Percentage = totalIncome > 0 ? (g.Sum(t => t.ConvertedAmount) / totalIncome) * 100 : 0
                };
            })
            .OrderByDescending(c => c.TotalAmount)
            .Take(5)
            .ToList();

        var topExpenseCategories = transactionList
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var category = categories.First(c => c.Id == g.Key);
                return new CategorySummary
                {
                    CategoryId = g.Key,
                    CategoryName = category.Name,
                    CategoryColor = category.Color,
                    CategoryIcon = category.Icon,
                    TotalAmount = g.Sum(t => t.ConvertedAmount),
                    TransactionCount = g.Count(),
                    Percentage = totalExpenses > 0 ? (g.Sum(t => t.ConvertedAmount) / totalExpenses) * 100 : 0
                };
            })
            .OrderByDescending(c => c.TotalAmount)
            .Take(5)
            .ToList();

        return new TransactionSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetAmount = totalIncome - totalExpenses,
            TotalTransactions = transactionList.Count,
            HomeCurrencyId = user.HomeCurrencyId.Value,
            HomeCurrencyCode = homeCurrency!.Code,
            HomeCurrencySymbol = homeCurrency.Symbol,
            TopIncomeCategories = topIncomeCategories,
            TopExpenseCategories = topExpenseCategories,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }
}