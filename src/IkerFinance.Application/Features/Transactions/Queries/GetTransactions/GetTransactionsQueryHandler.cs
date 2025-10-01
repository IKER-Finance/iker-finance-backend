using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Shared.DTOs.Transactions;

namespace IkerFinance.Application.Features.Transactions.Queries.GetTransactions;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, List<TransactionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions
            .Include(t => t.Currency)
            .Include(t => t.ConvertedCurrency)
            .Include(t => t.Category)
            .Where(t => t.UserId == request.UserId);

        if (request.StartDate.HasValue)
            query = query.Where(t => t.Date >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(t => t.Date <= request.EndDate.Value);

        if (request.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);

        if (request.CurrencyId.HasValue)
            query = query.Where(t => t.CurrencyId == request.CurrencyId.Value);

        var transactions = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            Amount = t.Amount,
            CurrencyId = t.CurrencyId,
            CurrencyCode = t.Currency.Code,
            CurrencySymbol = t.Currency.Symbol,
            ConvertedAmount = t.ConvertedAmount,
            ConvertedCurrencyId = t.ConvertedCurrencyId,
            ConvertedCurrencyCode = t.ConvertedCurrency.Code,
            ExchangeRate = t.ExchangeRate,
            Type = t.Type,
            Description = t.Description,
            Notes = t.Notes,
            Date = t.Date,
            CategoryId = t.CategoryId,
            CategoryName = t.Category.Name,
            CreatedAt = t.CreatedAt
        }).ToList();
    }
}