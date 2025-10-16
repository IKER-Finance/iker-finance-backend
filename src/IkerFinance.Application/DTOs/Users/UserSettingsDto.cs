namespace IkerFinance.Application.DTOs.Users;

public class UserSettingsDto
{
    public int? HomeCurrencyId { get; set; } // Read-only: displayed but cannot be updated
    public int? DefaultTransactionCurrencyId { get; set; }
    public string TimeZone { get; set; } = string.Empty;
}
