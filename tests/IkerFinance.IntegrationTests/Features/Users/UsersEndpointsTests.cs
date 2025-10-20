using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IkerFinance.Application.Features.Users.Commands.ChangePassword;
using IkerFinance.Application.Features.Users.Commands.UpdateUserProfile;
using IkerFinance.Application.Features.Users.Commands.UpdateUserSettings;
using IkerFinance.IntegrationTests.Infrastructure;

namespace IkerFinance.IntegrationTests.Features.Users;

public class UsersEndpointsTests : BaseIntegrationTest
{
    public UsersEndpointsTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProfile_WithAuthentication_ShouldReturnOk()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var response = await Client.GetAsync("/api/users/profile");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("test@example.com");
        content.Should().Contain("Test");
        content.Should().Contain("User");
    }

    [Fact]
    public async Task GetProfile_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        await ResetDatabase();

        var response = await Client.GetAsync("/api/users/profile");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_WithValidData_ShouldReturnOk()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var command = new UpdateUserProfileCommand
        {
            FirstName = "Updated",
            LastName = "Name"
        };

        var response = await PutAsync("/api/users/profile", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Updated");
        content.Should().Contain("Name");
    }

    [Fact]
    public async Task ChangePassword_WithValidData_ShouldReturnOk()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var command = new ChangePasswordCommand
        {
            CurrentPassword = "Test@123",
            NewPassword = "NewPassword@123"
        };

        var response = await PutAsync("/api/users/password", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("success");
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ShouldReturnBadRequest()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var command = new ChangePasswordCommand
        {
            CurrentPassword = "WrongPassword@123",
            NewPassword = "NewPassword@123"
        };

        var response = await PutAsync("/api/users/password", command);

        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task ChangePassword_WithWeakNewPassword_ShouldReturnBadRequest()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var command = new ChangePasswordCommand
        {
            CurrentPassword = "Test@123",
            NewPassword = "weak"
        };

        var response = await PutAsync("/api/users/password", command);

        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task GetSettings_WithAuthentication_ShouldReturnOk()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var response = await Client.GetAsync("/api/users/settings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("homeCurrencyId");
        content.Should().Contain("timeZone");
    }

    [Fact]
    public async Task UpdateSettings_WithValidData_ShouldReturnOk()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var command = new UpdateUserSettingsCommand
        {
            TimeZone = "America/New_York",
            DefaultTransactionCurrencyId = 1
        };

        var response = await PutAsync("/api/users/settings", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("America/New_York");
    }

    [Fact]
    public async Task UpdateSettings_WithInvalidCurrencyId_ShouldReturnBadRequest()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var command = new UpdateUserSettingsCommand
        {
            TimeZone = "UTC",
            DefaultTransactionCurrencyId = 9999
        };

        var response = await PutAsync("/api/users/settings", command);

        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateProfile_ThenGetProfile_ShouldReflectChanges()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var updateCommand = new UpdateUserProfileCommand
        {
            FirstName = "Jane",
            LastName = "Smith"
        };

        await PutAsync("/api/users/profile", updateCommand);
        var response = await Client.GetAsync("/api/users/profile");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Jane");
        content.Should().Contain("Smith");
    }
}
