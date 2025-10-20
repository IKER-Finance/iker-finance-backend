using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IkerFinance.Application.Features.Feedbacks.Commands.CreateFeedback;
using IkerFinance.Application.Features.Feedbacks.Commands.UpdateFeedbackStatus;
using IkerFinance.Domain.Enums;
using IkerFinance.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using IkerFinance.Application.Common.Identity;

namespace IkerFinance.IntegrationTests.Features.Feedbacks;

public class FeedbacksEndpointsTests : BaseIntegrationTest
{
    public FeedbacksEndpointsTests(IntegrationTestWebAppFactory factory) : base(factory)
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

    private class FeedbackResponse
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
    }
}
