using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.ExchangeRates;

namespace IkerFinance.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;

public sealed class UpdateExchangeRateCommandHandler : IRequestHandler<UpdateExchangeRateCommand, ExchangeRateDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateExchangeRateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRateDto> Handle(
        UpdateExchangeRateCommand request,
        CancellationToken cancellationToken)
    {
        var adminUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.AdminUserId, cancellationToken);

        if (adminUser == null)
            throw new NotFoundException("Admin User", request.AdminUserId);

        var exchangeRate = await _context.ExchangeRates
            .Include(er => er.FromCurrency)
            .Include(er => er.ToCurrency)
            .FirstOrDefaultAsync(er => er.Id == request.Id, cancellationToken);

        if (exchangeRate == null)
            throw new NotFoundException("Exchange Rate", request.Id);

        exchangeRate.Rate = request.Rate;
        exchangeRate.EffectiveDate = DateTime.SpecifyKind(request.EffectiveDate, DateTimeKind.Utc);
        exchangeRate.IsActive = request.IsActive;
        exchangeRate.LastUpdated = DateTime.UtcNow;
        exchangeRate.UpdatedByUserId = request.AdminUserId;
        exchangeRate.UpdatedAt = DateTime.UtcNow;

        _context.Update(exchangeRate);
        await _context.SaveChangesAsync(cancellationToken);

        return new ExchangeRateDto
        {
            Id = exchangeRate.Id,
            FromCurrencyId = exchangeRate.FromCurrencyId,
            FromCurrencyCode = exchangeRate.FromCurrency.Code,
            FromCurrencyName = exchangeRate.FromCurrency.Name,
            ToCurrencyId = exchangeRate.ToCurrencyId,
            ToCurrencyCode = exchangeRate.ToCurrency.Code,
            ToCurrencyName = exchangeRate.ToCurrency.Name,
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
