using IkerFinance.Application.Common.Identity;

namespace IkerFinance.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
    string? ValidateToken(string token);
}