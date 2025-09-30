using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Shared.DTOs.Transactions;

namespace IkerFinance.Application.Features.Transactions.Commands.UpdateTransaction;

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyConversionService _conversionService;

    public UpdateTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrencyConversionService conversionService)
    {
        _context = context;
        _conversionService = conversionService;
    }

    public async Task<TransactionDto> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Currency)
            .Include(t => t.ConvertedCurrency)
            .Include(t => t.Category)
            .Where(t => t.Id == request.Id && t.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (transaction == null)
            throw new NotFoundException("Transaction", request.Id);

        var category = await _context.Categories.FindAsync(new object[] { request.CategoryId }, cancellationToken);
        if (category == null)
            throw new NotFoundException("Category", request.CategoryId);

        var currency = await _context.Currencies.FindAsync(new object[] { request.CurrencyId }, cancellationToken);
        if (currency == null || !currency.IsActive)
            throw new ValidationException("Invalid or inactive currency");

        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        var homeCurrencyId = user!.HomeCurrencyId!.Value;

        decimal convertedAmount;
        decimal exchangeRate;

        if (request.CurrencyId == homeCurrencyId)
        {
            convertedAmount = request.Amount;
            exchangeRate = 1.0m;
        }
        else
        {
            var rateExists = await _conversionService.RatesExistAsync(request.CurrencyId, homeCurrencyId);
            if (!rateExists)
                throw new ValidationException("No exchange rate available for this currency");

            var rate = await _conversionService.GetExchangeRateAsync(request.CurrencyId, homeCurrencyId);
            exchangeRate = rate.Rate;
            convertedAmount = request.Amount * exchangeRate;
        }

        transaction.Amount = request.Amount;
        transaction.CurrencyId = request.CurrencyId;
        transaction.ConvertedAmount = convertedAmount;
        transaction.ConvertedCurrencyId = homeCurrencyId;
        transaction.ExchangeRate = exchangeRate;
        transaction.ExchangeRateDate = DateTime.UtcNow;
        transaction.Type = category.Type;
        transaction.Description = request.Description;
        transaction.Notes = request.Notes;
        transaction.Date = request.Date;
        transaction.CategoryId = request.CategoryId;
        transaction.UpdatedAt = DateTime.UtcNow;

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