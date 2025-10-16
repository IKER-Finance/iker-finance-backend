using System.Net;
using FluentAssertions;
using IkerFinance.IntegrationTests.Infrastructure;

namespace IkerFinance.IntegrationTests.Features.Currencies;

public class CurrenciesEndpointsTests : BaseIntegrationTest
{
    public CurrenciesEndpointsTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCurrencies_ShouldReturnListOfCurrencies()
    {
        var response = await Client.GetAsync("/api/currencies");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCurrencies_WithoutAuthentication_ShouldSucceed()
    {
        var response = await Client.GetAsync("/api/currencies");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
