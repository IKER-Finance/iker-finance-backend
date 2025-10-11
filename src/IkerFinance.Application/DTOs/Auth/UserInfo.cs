namespace IkerFinance.Application.DTOs.Auth;

public record UserInfo(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    int? HomeCurrencyId
);