using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IkerFinance.Application.Features.ExchangeRates.Commands.CreateExchangeRate;
using IkerFinance.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;
using IkerFinance.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using IkerFinance.Application.Common.Identity;

namespace IkerFinance.IntegrationTests.Features.ExchangeRates;

public class ExchangeRatesEndpointsTests : BaseIntegrationTest
{
    public ExchangeRatesEndpointsTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    private async Task<string> CreateAdminUserAsync()
    {
        await ResetDatabase();

        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Create Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Create admin user
        var adminUser = new ApplicationUser
        {
            UserName = "admin@test.com",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            HomeCurrencyId = 1,
            IsActive = true,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Admin");

        // Login and get token
        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(
            Client,
            "admin@test.com",
            "Admin@123",
            "Admin",
            "User",
            1
        );

        return token;
    }

    [Fact]
    public async Task CreateExchangeRate_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        await ResetDatabase();

        var command = new CreateExchangeRateCommand
        {
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = 1.25m,
            EffectiveDate = DateTime.UtcNow
        };

        var response = await PostAsync("/api/exchangerates", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private class ExchangeRateResponse
    {
        public int Id { get; set; }
        public decimal Rate { get; set; }
    }
}
