namespace IkerFinance.Infrastructure.Mobile.Identity;

public class MobileUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string HomeCurrencyCode { get; set; } = string.Empty;
    public string? PreferredLanguage { get; set; }
    public string? TimeZone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
