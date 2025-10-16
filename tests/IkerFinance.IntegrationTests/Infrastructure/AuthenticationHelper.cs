using System.Net.Http.Json;
using IkerFinance.Application.Features.Auth.Commands.Login;
using IkerFinance.Application.Features.Auth.Commands.Register;

namespace IkerFinance.IntegrationTests.Infrastructure;

public static class AuthenticationHelper
{
    public static async Task<string> RegisterAndLoginUserAsync(
        HttpClient client,
        string email = "test@example.com",
        string password = "Test@123",
        string firstName = "Test",
        string lastName = "User",
        int homeCurrencyId = 1)
    {
        var registerCommand = new RegisterCommand(
            email,
            password,
            password,
            firstName,
            lastName,
            homeCurrencyId
        );

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerCommand);

        if (!registerResponse.IsSuccessStatusCode)
        {
            var errorContent = await registerResponse.Content.ReadAsStringAsync();
            throw new Exception($"Registration failed: {errorContent}");
        }

        var loginCommand = new LoginCommand(email, password);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginCommand);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        if (loginResult?.Token == null)
        {
            throw new Exception("Login failed: No token received");
        }

        return loginResult.Token;
    }

    private class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
