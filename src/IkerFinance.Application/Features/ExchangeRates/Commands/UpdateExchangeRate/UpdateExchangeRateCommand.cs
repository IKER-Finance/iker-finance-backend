using IkerFinance.Application.DTOs.ExchangeRates;
using MediatR;

namespace IkerFinance.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;

public class UpdateExchangeRateCommand : IRequest<ExchangeRateDto>
{
    public int Id { get; set; }
    public string AdminUserId { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; }
}
