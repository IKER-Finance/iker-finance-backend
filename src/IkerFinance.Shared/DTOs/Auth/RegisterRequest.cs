using IkerFinance.Domain.Entities;

namespace IkerFinance.Shared.DTOs.Auth;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Currency HomeCurrency { get; set; } = Currency.SEK;
    public string? PreferredLanguage { get; set; }
}