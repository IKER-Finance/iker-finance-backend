using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Users;

namespace IkerFinance.Application.Features.Users.Queries.GetUserProfile;

public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;

    public GetUserProfileQueryHandler(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<UserProfileDto> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException("User", request.UserId);

        string? homeCurrencyCode = null;
        string? defaultTransactionCurrencyCode = null;

        if (user.HomeCurrencyId.HasValue)
        {
            var homeCurrency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Id == user.HomeCurrencyId.Value, cancellationToken);
            homeCurrencyCode = homeCurrency?.Code;
        }

        if (user.DefaultTransactionCurrencyId.HasValue)
        {
            var defaultCurrency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Id == user.DefaultTransactionCurrencyId.Value, cancellationToken);
            defaultTransactionCurrencyCode = defaultCurrency?.Code;
        }

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            HomeCurrencyId = user.HomeCurrencyId,
            HomeCurrencyCode = homeCurrencyCode,
            DefaultTransactionCurrencyId = user.DefaultTransactionCurrencyId,
            DefaultTransactionCurrencyCode = defaultTransactionCurrencyCode,
            TimeZone = user.TimeZone,
            PreferredLanguage = user.PreferredLanguage,
            IsActive = user.IsActive,
            CurrencySetupComplete = user.CurrencySetupComplete,
            RegistrationDate = user.RegistrationDate,
            LastLoginDate = user.LastLoginDate
        };
    }
}
