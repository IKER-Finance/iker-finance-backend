using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Extensions;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;
using IkerFinance.Shared.DTOs.Common;
using IkerFinance.Shared.DTOs.Transactions;

namespace IkerFinance.Application.Features.Transactions.Queries.GetTransactions;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PaginatedResponse<TransactionDto>>
{
    private readonly IApplicationDbContext _context;

    private readonly Dictionary<string, Expression<Func<Transaction, object>>> _sortExpressions = new()
    {
        { "Date", t => t.Date },
        { "Amount", t => t.Amount },
        { "Description", t => t.Description },
        { "CreatedAt", t => t.CreatedAt }
    };

    public GetTransactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<TransactionDto>> Handle(
        GetTransactionsQuery request, 
        CancellationToken cancellationToken)
    {
        request.ValidatePagination();

        var query = _context.Transactions
            .Include(t => t.Currency)
            .Include(t => t.ConvertedCurrency)
            .Include(t => t.Category)
            .Where(t => t.UserId == request.UserId);

        var searchTerm = request.GetNormalizedSearchTerm();
        if (searchTerm != null)
        {
            query = query.Where(t =>
                t.Description.ToLower().Contains(searchTerm) ||
                (t.Notes != null && t.Notes.ToLower().Contains(searchTerm)));
        }

        if (request.Type.HasValue)
            query = query.Where(t => t.Type == (TransactionType)request.Type.Value);

        if (request.StartDate.HasValue)
            query = query.Where(t => t.Date >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(t => t.Date <= request.EndDate.Value);

        if (request.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);

        if (request.CurrencyId.HasValue)
            query = query.Where(t => t.CurrencyId == request.CurrencyId.Value);

        query = query.ApplySorting(request.SortBy, request.SortOrder, _sortExpressions);

        return await query.ToPaginatedListAsync(
            request.PageNumber,
            request.PageSize,
            t => new TransactionDto
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
            },
            cancellationToken);
    }
}