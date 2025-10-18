using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.ExchangeRates;
using IkerFinance.Domain.Entities;

namespace IkerFinance.Application.Features.ExchangeRates.Commands.CreateExchangeRate;

public sealed class CreateExchangeRateCommandHandler : IRequestHandler<CreateExchangeRateCommand, ExchangeRateDto>
{
    private readonly IApplicationDbContext _context;

    public CreateExchangeRateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRateDto> Handle(
        CreateExchangeRateCommand request,
        CancellationToken cancellationToken)
    {
        // Verify admin user exists
        var adminUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.AdminUserId, cancellationToken);

        if (adminUser == null)
            throw new NotFoundException("Admin User", request.AdminUserId);

        // Verify currencies exist
        var fromCurrency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == request.FromCurrencyId, cancellationToken);

        if (fromCurrency == null)
            throw new NotFoundException("From Currency", request.FromCurrencyId);

        var toCurrency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == request.ToCurrencyId, cancellationToken);

        if (toCurrency == null)
            throw new NotFoundException("To Currency", request.ToCurrencyId);

        // Create exchange rate
        var exchangeRate = new ExchangeRate
        {
            FromCurrencyId = request.FromCurrencyId,
            ToCurrencyId = request.ToCurrencyId,
            Rate = request.Rate,
            EffectiveDate = DateTime.SpecifyKind(request.EffectiveDate, DateTimeKind.Utc),
            IsActive = request.IsActive,
            LastUpdated = DateTime.UtcNow,
            UpdatedByUserId = request.AdminUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Add(exchangeRate);
        await _context.SaveChangesAsync(cancellationToken);

        // Return DTO
        return new ExchangeRateDto
        {
            Id = exchangeRate.Id,
            FromCurrencyId = exchangeRate.FromCurrencyId,
            FromCurrencyCode = fromCurrency.Code,
            FromCurrencyName = fromCurrency.Name,
            ToCurrencyId = exchangeRate.ToCurrencyId,
            ToCurrencyCode = toCurrency.Code,
            ToCurrencyName = toCurrency.Name,
            Rate = exchangeRate.Rate,
            EffectiveDate = exchangeRate.EffectiveDate,
            IsActive = exchangeRate.IsActive,
            LastUpdated = exchangeRate.LastUpdated,
            UpdatedByUserId = exchangeRate.UpdatedByUserId,
            UpdatedByUserName = adminUser.FirstName + " " + adminUser.LastName,
            CreatedAt = exchangeRate.CreatedAt
        };
    }
}
