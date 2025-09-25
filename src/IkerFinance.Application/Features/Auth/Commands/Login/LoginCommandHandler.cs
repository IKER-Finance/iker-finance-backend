using MediatR;
using Microsoft.AspNetCore.Identity;
using IkerFinance.Domain.Entities;
using IkerFinance.Shared.DTOs.Auth;
using IkerFinance.Application.Common.Interfaces;

namespace IkerFinance.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        user.LastLoginDate = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = _tokenService.GenerateToken(user);

        return new AuthResponse(
            Token: token,
            RefreshToken: "",
            ExpiresAt: DateTime.UtcNow.AddHours(24),
            User: new UserInfo(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName
            )
        );
    }
}