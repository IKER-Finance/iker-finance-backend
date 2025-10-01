namespace IkerFinance.Shared.DTOs.Auth;

public record UserInfo(
    string Id,
    string Email,
    string FirstName,
    string LastName
);