using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;

namespace IkerFinance.Application.Features.ExchangeRates.Commands.DeleteExchangeRate;

public sealed class DeleteExchangeRateCommandHandler : IRequestHandler<DeleteExchangeRateCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteExchangeRateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        DeleteExchangeRateCommand request,
        CancellationToken cancellationToken)
    {
        var exchangeRate = await _context.ExchangeRates
            .FirstOrDefaultAsync(er => er.Id == request.Id, cancellationToken);

        if (exchangeRate == null)
            throw new NotFoundException("Exchange Rate", request.Id);

        _context.Remove(exchangeRate);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
