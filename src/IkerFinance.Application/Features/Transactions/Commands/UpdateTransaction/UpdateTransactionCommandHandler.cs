using MediatR;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Domain.DomainServices.Transaction;
using IkerFinance.Application.DTOs.Transactions;
using IkerFinance.Domain.Entities;
using IkerFinance.Application.Common.Identity;

namespace IkerFinance.Application.Features.Transactions.Commands.UpdateTransaction;

public sealed class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyConversionService _conversionService;
    private readonly TransactionService _transactionService;
    private readonly IReadRepository<Transaction> _transactionRepository;
    private readonly IReadRepository<ApplicationUser> _userRepository;
    private readonly IReadRepository<Category> _categoryRepository;
    private readonly IReadRepository<Currency> _currencyRepository;

    public UpdateTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrencyConversionService conversionService,
        TransactionService transactionService,
        IReadRepository<Transaction> transactionRepository,
        IReadRepository<ApplicationUser> userRepository,
        IReadRepository<Category> categoryRepository,
        IReadRepository<Currency> currencyRepository)
    {
        _context = context;
        _conversionService = conversionService;
        _transactionService = transactionService;
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _currencyRepository = currencyRepository;
    }

    public async Task<TransactionDto> Handle(
        UpdateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetAsync(
            t => t.Id == request.Id && t.UserId == request.UserId,
            cancellationToken);

        if (transaction == null)
            throw new NotFoundException("Transaction", request.Id);

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || !user.HomeCurrencyId.HasValue)
            throw new ValidationException("User home currency not set");

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
            throw new NotFoundException("Category", request.CategoryId);

        var currency = await _currencyRepository.GetByIdAsync(request.CurrencyId, cancellationToken);
        if (currency == null || !currency.IsActive)
            throw new ValidationException("Invalid or inactive currency");

        var homeCurrencyId = user.HomeCurrencyId.Value;

        var homeCurrency = await _currencyRepository.GetByIdAsync(homeCurrencyId, cancellationToken);

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