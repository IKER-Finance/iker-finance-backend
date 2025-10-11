using MediatR;
using Microsoft.AspNetCore.Identity;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Application.DTOs.Auth;
using IkerFinance.Application.Common.Interfaces;

namespace IkerFinance.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            HomeCurrencyId = request.HomeCurrencyId,
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ApplicationException($"Registration failed: {errors}");
        }

        var token = _tokenService.GenerateToken(user);

        return new AuthResponse(
            Token: token,
            RefreshToken: "", // Empty for now
            ExpiresAt: DateTime.UtcNow.AddHours(24),
            User: new UserInfo(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                user.HomeCurrencyId
            )
        );
    }
}