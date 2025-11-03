using IkerFinance.Infrastructure.Mobile.Identity;

namespace IkerFinance.Infrastructure.Mobile.Services;

public interface IMobileAuthService
{
    Task<(bool Success, MobileUser? User, string? ErrorMessage)> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string homeCurrencyCode);

    Task<(bool Success, MobileUser? User, string? ErrorMessage)> LoginAsync(
        string email,
        string password);

    Task<MobileUser?> GetCurrentUserAsync();

    Task LogoutAsync();

    Task<bool> IsAuthenticatedAsync();

    string? GetCurrentUserId();
}
