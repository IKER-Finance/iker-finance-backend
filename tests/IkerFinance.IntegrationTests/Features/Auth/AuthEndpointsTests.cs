using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IkerFinance.Application.Features.Auth.Commands.Login;
using IkerFinance.Application.Features.Auth.Commands.Register;
using IkerFinance.IntegrationTests.Infrastructure;

namespace IkerFinance.IntegrationTests.Features.Auth;

public class AuthEndpointsTests : BaseIntegrationTest
{
    public AuthEndpointsTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        await ResetDatabase();

        var currency = DbContext.Currencies.First();
        var command = new RegisterCommand(
            "newuser@example.com",
            "Test@123",
            "Test@123",
            "John",
            "Doe",
            currency.Id
        );

        var response = await PostAsync("/api/auth/register", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        await ResetDatabase();

        var currency = DbContext.Currencies.First();
        var command = new RegisterCommand(
            "duplicate@example.com",
            "Test@123",
            "Test@123",
            "John",
            "Doe",
            currency.Id
        );

        await PostAsync("/api/auth/register", command);

        var duplicateResponse = await PostAsync("/api/auth/register", command);

        duplicateResponse.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact(Skip = "Password mismatch validation not implemented in API yet")]
    public async Task Register_WithMismatchedPasswords_ShouldReturnBadRequest()
    {
        await ResetDatabase();

        var currency = DbContext.Currencies.First();
        var command = new RegisterCommand(
            "test@example.com",
            "Test@123",
            "Different@123",
            "John",
            "Doe",
            currency.Id
        );

        var response = await PostAsync("/api/auth/register", command);

        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        await ResetDatabase();

        var currency = DbContext.Currencies.First();
        var registerCommand = new RegisterCommand(
            "login@example.com",
            "Test@123",
            "Test@123",
            "John",
            "Doe",
            currency.Id
        );

        await PostAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginCommand("login@example.com", "Test@123");

        var response = await PostAsync("/api/auth/login", loginCommand);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("token");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        await ResetDatabase();

        var loginCommand = new LoginCommand("nonexistent@example.com", "WrongPassword@123");

        var response = await PostAsync("/api/auth/login", loginCommand);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        await ResetDatabase();

        var currency = DbContext.Currencies.First();
        var registerCommand = new RegisterCommand(
            "wrongpass@example.com",
            "Test@123",
            "Test@123",
            "John",
            "Doe",
            currency.Id
        );

        await PostAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginCommand("wrongpass@example.com", "WrongPassword@123");

        var response = await PostAsync("/api/auth/login", loginCommand);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
