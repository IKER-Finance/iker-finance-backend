using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Users;

namespace IkerFinance.Application.Features.Users.Commands.UpdateUserSettings;

public sealed class UpdateUserSettingsCommandHandler : IRequestHandler<UpdateUserSettingsCommand, UserSettingsDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;

    public UpdateUserSettingsCommandHandler(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<UserSettingsDto> Handle(
        UpdateUserSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException("User", request.UserId);

        // Validate default transaction currency if provided
        if (request.DefaultTransactionCurrencyId.HasValue)
        {
            var defaultCurrency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Id == request.DefaultTransactionCurrencyId.Value, cancellationToken);
            if (defaultCurrency == null || !defaultCurrency.IsActive)
                throw new ValidationException("Invalid or inactive default transaction currency");
        }

        // Update only safe settings (NOT HomeCurrency - would break system)
        user.DefaultTransactionCurrencyId = request.DefaultTransactionCurrencyId;
        user.TimeZone = request.TimeZone;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ApplicationException($"Update failed: {errors}");
        }

        return new UserSettingsDto
        {
            HomeCurrencyId = user.HomeCurrencyId,
            DefaultTransactionCurrencyId = user.DefaultTransactionCurrencyId,
            TimeZone = user.TimeZone
        };
    }
}
