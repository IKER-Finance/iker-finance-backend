using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Domain.Enums;
using IkerFinance.Shared.DTOs.Transactions;

namespace IkerFinance.Application.Features.Transactions.Queries.GetTransactionSummary;

public class GetTransactionSummaryQueryHandler : IRequestHandler<GetTransactionSummaryQuery, TransactionSummaryDto>
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
            .Include(u => u.HomeCurrency)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user?.HomeCurrencyId == null)
            throw new NotFoundException("User", request.UserId);

        var query = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == request.UserId);

        if (request.StartDate.HasValue)
        {
            var startDateUtc = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(t => t.Date >= startDateUtc);
        }

        if (request.EndDate.HasValue)
        {
            var endDateUtc = DateTime.SpecifyKind(request.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(t => t.Date <= endDateUtc);
        }

        var transactions = await query.ToListAsync(cancellationToken);

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.ConvertedAmount);

        var totalExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.ConvertedAmount);

        var topIncomeCategories = transactions
            .Where(t => t.Type == TransactionType.Income)
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color, t.Category.Icon })
            .Select(g => new CategorySummary
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                CategoryColor = g.Key.Color,
                CategoryIcon = g.Key.Icon,
                TotalAmount = g.Sum(t => t.ConvertedAmount),
                TransactionCount = g.Count(),
                Percentage = totalIncome > 0 ? (g.Sum(t => t.ConvertedAmount) / totalIncome) * 100 : 0
            })
            .OrderByDescending(c => c.TotalAmount)
            .Take(5)
            .ToList();

        var topExpenseCategories = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color, t.Category.Icon })
            .Select(g => new CategorySummary
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                CategoryColor = g.Key.Color,
                CategoryIcon = g.Key.Icon,
                TotalAmount = g.Sum(t => t.ConvertedAmount),
                TransactionCount = g.Count(),
                Percentage = totalExpenses > 0 ? (g.Sum(t => t.ConvertedAmount) / totalExpenses) * 100 : 0
            })
            .OrderByDescending(c => c.TotalAmount)
            .Take(5)
            .ToList();

        return new TransactionSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetAmount = totalIncome - totalExpenses,
            TotalTransactions = transactions.Count,
            HomeCurrencyId = user.HomeCurrencyId.Value,
            HomeCurrencyCode = user.HomeCurrency!.Code,
            HomeCurrencySymbol = user.HomeCurrency.Symbol,
            TopIncomeCategories = topIncomeCategories,
            TopExpenseCategories = topExpenseCategories,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }
}