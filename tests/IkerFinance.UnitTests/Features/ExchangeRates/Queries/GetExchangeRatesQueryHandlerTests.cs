using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.ExchangeRates;
using IkerFinance.Application.Features.ExchangeRates.Queries.GetExchangeRates;
using IkerFinance.Domain.Entities;

namespace IkerFinance.UnitTests.Application.Features.ExchangeRates.Queries;

public class GetExchangeRatesQueryHandlerTests
{
    private readonly Mock<IExchangeRateRepository> _mockRepo;
    private readonly GetExchangeRatesQueryHandler _handler;

    public GetExchangeRatesQueryHandlerTests()
    {
        _mockRepo = new Mock<IExchangeRateRepository>();
        _handler = new GetExchangeRatesQueryHandler(_mockRepo.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnExpectedPagedResult()
    {
        var query = new GetExchangeRatesQuery { PageNumber = 1, PageSize = 5 };
        var expected = new PaginatedResponse<ExchangeRateDto>
        {
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 5,
            Data = new List<ExchangeRateDto>
            {
                new ExchangeRateDto { FromCurrencyCode = "USD", ToCurrencyCode = "EUR", Rate = 0.95m },
                new ExchangeRateDto { FromCurrencyCode = "EUR", ToCurrencyCode = "JPY", Rate = 1.05m }
            }
        };

        _mockRepo.Setup(x => x.GetExchangeRatesAsync(It.IsAny<ExchangeRateFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        // 支持两种可能的返回类型：PaginatedResponse<ExchangeRateDto> 或 IEnumerable<ExchangeRateDto>
        if (result is PaginatedResponse<ExchangeRateDto> paginated)
        {
            paginated.Data.Should().HaveCount(2);
            paginated.Data.First().FromCurrencyCode.Should().Be("USD");
        }
        else if (result is IEnumerable<ExchangeRateDto> seq)
        {
            var list = seq.ToList();
            list.Should().HaveCount(2);
            list.First().FromCurrencyCode.Should().Be("USD");
        }
        else
        {
            // 失败时给出明确信息，便于调试实际返回类型
            result.Should().BeAssignableTo<IEnumerable<ExchangeRateDto>>($"Handler returned unexpected type: {result?.GetType().FullName ?? "null"}");
        }

        _mockRepo.Verify(x => x.GetExchangeRatesAsync(It.IsAny<ExchangeRateFilters>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoData_ShouldReturnEmptyDataList()
    {
        var query = new GetExchangeRatesQuery { PageNumber = 1, PageSize = 10 };

        _mockRepo.Setup(x => x.GetExchangeRatesAsync(It.IsAny<ExchangeRateFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResponse<ExchangeRateDto>
            {
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10,
                Data = new List<ExchangeRateDto>()
            });

        var result = await _handler.Handle(query, CancellationToken.None);

        if (result is PaginatedResponse<ExchangeRateDto> paginated)
        {
            paginated.Data.Should().BeEmpty();
            paginated.TotalCount.Should().Be(0);
        }
        else if (result is IEnumerable<ExchangeRateDto> seq)
        {
            seq.Should().BeEmpty();
        }
        else
        {
            result.Should().BeAssignableTo<IEnumerable<ExchangeRateDto>>();
        }
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectFilterParametersToRepository()
    {
        ExchangeRateFilters? captured = null;

        _mockRepo.Setup(x => x.GetExchangeRatesAsync(It.IsAny<ExchangeRateFilters>(), It.IsAny<CancellationToken>()))
            .Callback<ExchangeRateFilters, CancellationToken>((filters, _) => captured = filters)
            .ReturnsAsync(new PaginatedResponse<ExchangeRateDto>());

        var query = new GetExchangeRatesQuery
        {
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            IsActive = true
        };

        await _handler.Handle(query, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.FromCurrencyId.Should().Be(1);
        captured.ToCurrencyId.Should().Be(2);
        captured.IsActive.Should().BeTrue();
    }
}
