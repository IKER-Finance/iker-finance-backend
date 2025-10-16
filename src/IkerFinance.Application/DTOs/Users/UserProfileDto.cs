namespace IkerFinance.Application.DTOs.Users;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? HomeCurrencyId { get; set; }
    public string? HomeCurrencyCode { get; set; }
    public int? DefaultTransactionCurrencyId { get; set; }
    public string? DefaultTransactionCurrencyCode { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public string? PreferredLanguage { get; set; }
    public bool IsActive { get; set; }
    public bool CurrencySetupComplete { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
}
