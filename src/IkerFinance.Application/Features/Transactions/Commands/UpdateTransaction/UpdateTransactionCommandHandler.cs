using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Domain.Services;
using IkerFinance.Shared.DTOs.Transactions;

namespace IkerFinance.Application.Features.Transactions.Commands.UpdateTransaction;

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyConversionService _conversionService;
    private readonly TransactionService _transactionService;

    public UpdateTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrencyConversionService conversionService)
    {
        _context = context;
        _conversionService = conversionService;
        _transactionService = new TransactionService();
    }

    public async Task<TransactionDto> Handle(
        UpdateTransactionCommand request, 
        CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .Include(t => t.User)
            .Include(t => t.Currency)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == request.UserId, cancellationToken);

        if (transaction == null)
            throw new NotFoundException("Transaction", request.Id);

        var user = transaction.User;
        if (!user.HomeCurrencyId.HasValue)
            throw new ValidationException("User home currency not set");

        var category = await _context.Categories.FindAsync(new object[] { request.CategoryId }, cancellationToken);
        if (category == null)
            throw new NotFoundException("Category", request.CategoryId);

        var currency = await _context.Currencies.FindAsync(new object[] { request.CurrencyId }, cancellationToken);
        if (currency == null || !currency.IsActive)
            throw new ValidationException("Invalid or inactive currency");

        var homeCurrencyId = user.HomeCurrencyId.Value;

        Domain.Entities.ExchangeRate? exchangeRate = null;
        if (request.CurrencyId != homeCurrencyId)
        {
            var rateExists = await _conversionService.RatesExistAsync(request.CurrencyId, homeCurrencyId);
            if (!rateExists)
                throw new ValidationException("No exchange rate available for this currency");

            exchangeRate = await _conversionService.GetExchangeRateAsync(request.CurrencyId, homeCurrencyId);
        }

        _transactionService.Update(
            transaction: transaction,
            amount: request.Amount,
            currencyId: request.CurrencyId,
            homeCurrencyId: homeCurrencyId,
            categoryId: request.CategoryId,
            type: category.Type,
            description: request.Description,
            notes: request.Notes,
            date: request.Date,
            exchangeRate: exchangeRate
        );

        await _context.SaveChangesAsync(cancellationToken);

        return new TransactionDto
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            CurrencyId = transaction.CurrencyId,
            CurrencyCode = currency.Code,
            CurrencySymbol = currency.Symbol,
            ConvertedAmount = transaction.ConvertedAmount,
            ConvertedCurrencyId = transaction.ConvertedCurrencyId,
            ConvertedCurrencyCode = user.HomeCurrency!.Code,
            ExchangeRate = transaction.ExchangeRate,
            Type = transaction.Type,
            Description = transaction.Description,
            Notes = transaction.Notes,
            Date = transaction.Date,
            CategoryId = transaction.CategoryId,
            CategoryName = category.Name,
            CreatedAt = transaction.CreatedAt
        };
    }
}