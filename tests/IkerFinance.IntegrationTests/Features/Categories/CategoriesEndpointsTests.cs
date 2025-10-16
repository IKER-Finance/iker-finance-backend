using System.Net;
using FluentAssertions;
using IkerFinance.IntegrationTests.Infrastructure;

namespace IkerFinance.IntegrationTests.Features.Categories;

public class CategoriesEndpointsTests : BaseIntegrationTest
{
    public CategoriesEndpointsTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCategories_WithAuthentication_ShouldReturnSuccess()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(Client);
        SetAuthenticationToken(token);

        var response = await Client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCategories_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await Client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCategories_WithValidToken_ShouldReturnUserSpecificCategories()
    {
        await ResetDatabase();

        var token = await AuthenticationHelper.RegisterAndLoginUserAsync(
            Client,
            "user1@example.com",
            "Test@123",
            "User",
            "One");
        SetAuthenticationToken(token);

        var response = await Client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }
}
