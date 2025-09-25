namespace IkerFinance.Shared.DTOs.Auth;

public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfo User
);