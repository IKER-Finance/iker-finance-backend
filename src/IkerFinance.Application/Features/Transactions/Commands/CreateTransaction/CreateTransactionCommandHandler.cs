using MediatR;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Domain.DomainServices.Transaction;
using IkerFinance.Application.DTOs.Transactions;
using Microsoft.EntityFrameworkCore;

namespace IkerFinance.Application.Features.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyConversionService _conversionService;
    private readonly TransactionFactory _transactionFactory;

    public CreateTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrencyConversionService conversionService,
        TransactionFactory transactionFactory)
    {
        _context = context;
        _conversionService = conversionService;
        _transactionFactory = transactionFactory;
    }

    public async Task<TransactionDto> Handle(
        CreateTransactionCommand request, 
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null || !user.HomeCurrencyId.HasValue)
            throw new NotFoundException("User", request.UserId);

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (category == null)
            throw new NotFoundException("Category", request.CategoryId);

        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == request.CurrencyId, cancellationToken);
        if (currency == null || !currency.IsActive)
            throw new ValidationException("Invalid or inactive currency");

        var homeCurrencyId = user.HomeCurrencyId.Value;

        var homeCurrency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == homeCurrencyId, cancellationToken);

        Domain.Entities.ExchangeRate? exchangeRate = null;
        if (request.CurrencyId != homeCurrencyId)
        {
            var rateExists = await _conversionService.RatesExistAsync(request.CurrencyId, homeCurrencyId);
            if (!rateExists)
                throw new ValidationException("No exchange rate available for this currency");

            exchangeRate = await _conversionService.GetExchangeRateAsync(request.CurrencyId, homeCurrencyId);
        }

        var transaction = _transactionFactory.Create(
            userId: request.UserId,
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

        _context.Add(transaction);
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
            ConvertedCurrencyCode = homeCurrency!.Code,
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