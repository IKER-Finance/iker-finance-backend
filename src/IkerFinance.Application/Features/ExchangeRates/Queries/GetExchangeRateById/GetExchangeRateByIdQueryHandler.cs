using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.ExchangeRates;

namespace IkerFinance.Application.Features.ExchangeRates.Queries.GetExchangeRateById;

public sealed class GetExchangeRateByIdQueryHandler : IRequestHandler<GetExchangeRateByIdQuery, ExchangeRateDto>
{
    private readonly IApplicationDbContext _context;

    public GetExchangeRateByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRateDto> Handle(
        GetExchangeRateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var exchangeRate = await _context.ExchangeRates
            .Include(er => er.FromCurrency)
            .Include(er => er.ToCurrency)
            .Where(er => er.Id == request.Id)
            .GroupJoin(
                _context.Users,
                er => er.UpdatedByUserId,
                u => u.Id,
                (er, users) => new { ExchangeRate = er, UpdatedBy = users.FirstOrDefault() }
            )
            .Select(x => new ExchangeRateDto
            {
                Id = x.ExchangeRate.Id,
                FromCurrencyId = x.ExchangeRate.FromCurrencyId,
                FromCurrencyCode = x.ExchangeRate.FromCurrency.Code,
                FromCurrencyName = x.ExchangeRate.FromCurrency.Name,
                ToCurrencyId = x.ExchangeRate.ToCurrencyId,
                ToCurrencyCode = x.ExchangeRate.ToCurrency.Code,
                ToCurrencyName = x.ExchangeRate.ToCurrency.Name,
                Rate = x.ExchangeRate.Rate,
                EffectiveDate = x.ExchangeRate.EffectiveDate,
                IsActive = x.ExchangeRate.IsActive,
                LastUpdated = x.ExchangeRate.LastUpdated,
                UpdatedByUserId = x.ExchangeRate.UpdatedByUserId,
                UpdatedByUserName = x.UpdatedBy != null ? x.UpdatedBy.FirstName + " " + x.UpdatedBy.LastName : null,
                CreatedAt = x.ExchangeRate.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (exchangeRate == null)
            throw new NotFoundException("Exchange Rate", request.Id);

        return exchangeRate;
    }
}
