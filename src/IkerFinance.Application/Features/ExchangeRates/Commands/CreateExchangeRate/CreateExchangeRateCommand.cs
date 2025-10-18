using IkerFinance.Application.DTOs.ExchangeRates;
using MediatR;

namespace IkerFinance.Application.Features.ExchangeRates.Commands.CreateExchangeRate;

public class CreateExchangeRateCommand : IRequest<ExchangeRateDto>
{
    public string AdminUserId { get; set; } = string.Empty;
    public int FromCurrencyId { get; set; }
    public int ToCurrencyId { get; set; }
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; } = true;
}
