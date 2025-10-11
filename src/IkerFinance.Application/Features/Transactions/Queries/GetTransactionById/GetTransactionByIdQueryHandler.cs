using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.DTOs.Transactions;

namespace IkerFinance.Application.Features.Transactions.Queries.GetTransactionById;

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Currency)
            .Include(t => t.ConvertedCurrency)
            .Include(t => t.Category)
            .Where(t => t.Id == request.Id && t.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (transaction == null)
            throw new NotFoundException("Transaction", request.Id);

        return new TransactionDto
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            CurrencyId = transaction.CurrencyId,
            CurrencyCode = transaction.Currency.Code,
            CurrencySymbol = transaction.Currency.Symbol,
            ConvertedAmount = transaction.ConvertedAmount,
            ConvertedCurrencyId = transaction.ConvertedCurrencyId,
            ConvertedCurrencyCode = transaction.ConvertedCurrency.Code,
            ExchangeRate = transaction.ExchangeRate,
            Type = transaction.Type,
            Description = transaction.Description,
            Notes = transaction.Notes,
            Date = transaction.Date,
            CategoryId = transaction.CategoryId,
            CategoryName = transaction.Category.Name,
            CreatedAt = transaction.CreatedAt
        };
    }
}