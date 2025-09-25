using IkerFinance.Domain.Entities;

namespace IkerFinance.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
    string? ValidateToken(string token);
}