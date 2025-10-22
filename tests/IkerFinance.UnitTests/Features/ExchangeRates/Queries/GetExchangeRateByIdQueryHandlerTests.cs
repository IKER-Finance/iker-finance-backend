using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.ExchangeRates.Queries.GetExchangeRateById;
using IkerFinance.Domain.Entities;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

namespace IkerFinance.UnitTests.Application.Features.ExchangeRates.Queries;

public class GetExchangeRateByIdQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly GetExchangeRateByIdQueryHandler _handler;

    public GetExchangeRateByIdQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new GetExchangeRateByIdQueryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnExchangeRate()
    {
        var from = new Currency { Id = 1, Code = "USD", Name = "Dollar" };
        var to = new Currency { Id = 2, Code = "EUR", Name = "Euro" };

        var rateEntity = new ExchangeRate
        {
            Id = 1,
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = 1.15m,
            FromCurrency = from,
            ToCurrency = to
        };

        var ratesMock = new List<ExchangeRate> { rateEntity }.AsQueryable().BuildMock();
        var ratesDb = ratesMock.BuildMockDbSet();

        _mockContext.Setup(x => x.ExchangeRates).Returns(ratesDb.Object);

        var result = await _handler.Handle(new GetExchangeRateByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Rate.Should().Be(1.15m);
    }

    [Fact]
    public async Task Handle_WhenIdNotFound_ShouldThrowNotFoundException()
    {
        var ratesMock = new List<ExchangeRate>().AsQueryable().BuildMock();
        var ratesDb = ratesMock.BuildMockDbSet();
        _mockContext.Setup(x => x.ExchangeRates).Returns(ratesDb.Object);

        var act = async () => await _handler.Handle(new GetExchangeRateByIdQuery { Id = 999 }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
