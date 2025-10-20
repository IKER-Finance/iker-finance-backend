using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.ExchangeRates.Commands.DeleteExchangeRate;
using IkerFinance.Domain.Entities;

namespace IkerFinance.UnitTests.Application.Features.ExchangeRates.Commands;

public class DeleteExchangeRateCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly DeleteExchangeRateCommandHandler _handler;

    public DeleteExchangeRateCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new DeleteExchangeRateCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_WithExistingExchangeRate_ShouldDeleteSuccessfully()
    {
        var rate = new ExchangeRate { Id = 1, FromCurrencyId = 1, ToCurrencyId = 2, Rate = 1.05m };
        var rates = new List<ExchangeRate> { rate }.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.ExchangeRates).Returns(rates.Object);
        _mockContext.Setup(x => x.Remove(It.IsAny<ExchangeRate>()));
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteExchangeRateCommand { Id = 1 };
        await _handler.Handle(command, CancellationToken.None);

        _mockContext.Verify(x => x.Remove(It.Is<ExchangeRate>(r => r.Id == 1)), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenExchangeRateNotFound_ShouldThrowNotFoundException()
    {
        var rates = new List<ExchangeRate>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.ExchangeRates).Returns(rates.Object);

        var command = new DeleteExchangeRateCommand { Id = 999 };
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
