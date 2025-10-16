using FluentAssertions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Currencies.Queries.GetCurrencies;
using IkerFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.Currencies.Queries;

public class GetCurrenciesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly GetCurrenciesQueryHandler _handler;

    public GetCurrenciesQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new GetCurrenciesQueryHandler(_mockContext.Object);
    }

    // Test: Returns only active currencies ordered by code
    [Fact]
    public async Task Handle_ReturnsOnlyActiveCurrenciesOrderedByCode()
    {
        var currencies = new List<Currency>
        {
            new() { Id = 1, Code = "USD", Name = "US Dollar", Symbol = "$", IsActive = true },
            new() { Id = 2, Code = "EUR", Name = "Euro", Symbol = "€", IsActive = true },
            new() { Id = 3, Code = "GBP", Name = "British Pound", Symbol = "£", IsActive = false },
            new() { Id = 4, Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", IsActive = true }
        };

        var mockDbSet = CreateMockDbSet(currencies);
        _mockContext.Setup(x => x.Currencies).Returns(mockDbSet.Object);

        var query = new GetCurrenciesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().OnlyContain(c => c.Code == "USD" || c.Code == "EUR" || c.Code == "CAD");
        result[0].Code.Should().Be("CAD");
        result[1].Code.Should().Be("EUR");
        result[2].Code.Should().Be("USD");
    }

    // Test: Returns empty list when no active currencies exist
    [Fact]
    public async Task Handle_WhenNoActiveCurrencies_ReturnsEmptyList()
    {
        var currencies = new List<Currency>
        {
            new() { Id = 1, Code = "USD", Name = "US Dollar", Symbol = "$", IsActive = false },
            new() { Id = 2, Code = "EUR", Name = "Euro", Symbol = "€", IsActive = false }
        };

        var mockDbSet = CreateMockDbSet(currencies);
        _mockContext.Setup(x => x.Currencies).Returns(mockDbSet.Object);

        var query = new GetCurrenciesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    // Test: Maps currency properties correctly to DTO
    [Fact]
    public async Task Handle_MapsCurrencyPropertiesToDto()
    {
        var currencies = new List<Currency>
        {
            new() { Id = 5, Code = "JPY", Name = "Japanese Yen", Symbol = "¥", IsActive = true }
        };

        var mockDbSet = CreateMockDbSet(currencies);
        _mockContext.Setup(x => x.Currencies).Returns(mockDbSet.Object);

        var query = new GetCurrenciesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(5);
        result[0].Code.Should().Be("JPY");
        result[0].Name.Should().Be("Japanese Yen");
        result[0].Symbol.Should().Be("¥");
    }

    // Test: Returns all active currencies when multiple exist
    [Fact]
    public async Task Handle_ReturnsAllActiveCurrencies()
    {
        var currencies = new List<Currency>
        {
            new() { Id = 1, Code = "USD", Name = "US Dollar", Symbol = "$", IsActive = true },
            new() { Id = 2, Code = "EUR", Name = "Euro", Symbol = "€", IsActive = true },
            new() { Id = 3, Code = "GBP", Name = "British Pound", Symbol = "£", IsActive = true },
            new() { Id = 4, Code = "JPY", Name = "Japanese Yen", Symbol = "¥", IsActive = true },
            new() { Id = 5, Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", IsActive = true }
        };

        var mockDbSet = CreateMockDbSet(currencies);
        _mockContext.Setup(x => x.Currencies).Returns(mockDbSet.Object);

        var query = new GetCurrenciesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(5);
    }

    // Test: Sorting is case-sensitive and follows string ordering
    [Fact]
    public async Task Handle_SortsByCodeAlphabetically()
    {
        var currencies = new List<Currency>
        {
            new() { Id = 1, Code = "ZAR", Name = "South African Rand", Symbol = "R", IsActive = true },
            new() { Id = 2, Code = "AUD", Name = "Australian Dollar", Symbol = "A$", IsActive = true },
            new() { Id = 3, Code = "MXN", Name = "Mexican Peso", Symbol = "$", IsActive = true }
        };

        var mockDbSet = CreateMockDbSet(currencies);
        _mockContext.Setup(x => x.Currencies).Returns(mockDbSet.Object);

        var query = new GetCurrenciesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result[0].Code.Should().Be("AUD");
        result[1].Code.Should().Be("MXN");
        result[2].Code.Should().Be("ZAR");
    }

    private static Mock<DbSet<Currency>> CreateMockDbSet(List<Currency> data)
    {
        return data.AsQueryable().BuildMockDbSet();
    }
}
