using Microsoft.EntityFrameworkCore;
using IkerFinance.Infrastructure.Mobile.Data;
using IkerFinance.Infrastructure.Mobile.Identity;

namespace IkerFinance.Infrastructure.Mobile.Services;

public class MobileAuthService : IMobileAuthService
{
    private readonly MobileDbContext _context;
    private string? _currentUserId;

    public MobileAuthService(MobileDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, MobileUser? User, string? ErrorMessage)> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string homeCurrencyCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, null, "Email is required");

            if (string.IsNullOrWhiteSpace(password))
                return (false, null, "Password is required");

            if (password.Length < 6)
                return (false, null, "Password must be at least 6 characters");

            if (string.IsNullOrWhiteSpace(firstName))
                return (false, null, "First name is required");

            if (string.IsNullOrWhiteSpace(lastName))
                return (false, null, "Last name is required");

            var existingUser = await _context.Set<MobileUser>()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (existingUser != null)
                return (false, null, "Email already registered");

            var currency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Code == homeCurrencyCode);

            if (currency == null)
                return (false, null, "Invalid currency code");

            var user = new MobileUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = email.ToLower(),
                FirstName = firstName,
                LastName = lastName,
                HomeCurrencyCode = homeCurrencyCode,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                PreferredLanguage = "en",
                TimeZone = "UTC"
            };

            _context.Set<MobileUser>().Add(user);
            await _context.SaveChangesAsync();

            _currentUserId = user.Id;

            return (true, user, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Registration failed: {ex.Message}");
        }
    }

    public async Task<(bool Success, MobileUser? User, string? ErrorMessage)> LoginAsync(
        string email,
        string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, null, "Email is required");

            if (string.IsNullOrWhiteSpace(password))
                return (false, null, "Password is required");

            var user = await _context.Set<MobileUser>()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
                return (false, null, "Invalid email or password");

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return (false, null, "Invalid email or password");

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _currentUserId = user.Id;

            return (true, user, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Login failed: {ex.Message}");
        }
    }

    public async Task<MobileUser?> GetCurrentUserAsync()
    {
        if (string.IsNullOrEmpty(_currentUserId))
            return null;

        return await _context.Set<MobileUser>()
            .FirstOrDefaultAsync(u => u.Id == _currentUserId);
    }

    public Task LogoutAsync()
    {
        _currentUserId = null;
        return Task.CompletedTask;
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(_currentUserId));
    }

    public string? GetCurrentUserId()
    {
        return _currentUserId;
    }
}
