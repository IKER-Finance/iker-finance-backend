using Microsoft.AspNetCore.Identity;

namespace IkerFinance.Application.Common.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public int? HomeCurrencyId { get; set; }
    public int? DefaultTransactionCurrencyId { get; set; }

    public string TimeZone { get; set; } = "Europe/Stockholm";
    public string? PreferredLanguage { get; set; }
    public bool IsActive { get; set; } = true;
    public bool CurrencySetupComplete { get; set; } = false;

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
}
