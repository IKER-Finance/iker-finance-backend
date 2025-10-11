using MediatR;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Domain.Enums;
using IkerFinance.Application.DTOs.Transactions;
using IkerFinance.Domain.Entities;
using IkerFinance.Application.Common.Identity;

namespace IkerFinance.Application.Features.Transactions.Queries.GetTransactionSummary;

public sealed class GetTransactionSummaryQueryHandler : IRequestHandler<GetTransactionSummaryQuery, TransactionSummaryDto>
{
    private readonly IReadRepository<ApplicationUser> _userRepository;
    private readonly IReadRepository<Currency> _currencyRepository;
    private readonly IReadRepository<Transaction> _transactionRepository;
    private readonly IReadRepository<Category> _categoryRepository;

    public GetTransactionSummaryQueryHandler(
        IReadRepository<ApplicationUser> userRepository,
        IReadRepository<Currency> currencyRepository,
        IReadRepository<Transaction> transactionRepository,
        IReadRepository<Category> categoryRepository)
    {
        _userRepository = userRepository;
        _currencyRepository = currencyRepository;
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<TransactionSummaryDto> Handle(
        GetTransactionSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user?.HomeCurrencyId == null)
            throw new NotFoundException("User", request.UserId);

        var homeCurrency = await _currencyRepository.GetByIdAsync(user.HomeCurrencyId.Value, cancellationToken);

        var transactions = await _transactionRepository.FindAsync(
            t => t.UserId == request.UserId,
            cancellationToken);

        var filteredTransactions = transactions.AsEnumerable();

        if (request.StartDate.HasValue)
        {
            var startDateUtc = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
            filteredTransactions = filteredTransactions.Where(t => t.Date >= startDateUtc);
        }

        if (request.EndDate.HasValue)
        {
            var endDateUtc = DateTime.SpecifyKind(request.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            filteredTransactions = filteredTransactions.Where(t => t.Date <= endDateUtc);
        }

        var transactionList = filteredTransactions.ToList();

        var categoryIds = transactionList.Select(t => t.CategoryId).Distinct().ToList();
        var categoriesResult = await _categoryRepository.FindAsync(
            c => categoryIds.Contains(c.Id),
            cancellationToken);
        var categories = categoriesResult.ToList();

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