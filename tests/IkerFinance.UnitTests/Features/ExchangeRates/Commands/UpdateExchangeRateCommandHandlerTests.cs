using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;
using IkerFinance.Domain.Entities;

namespace IkerFinance.UnitTests.Application.Features.ExchangeRates.Commands
{
    public class UpdateExchangeRateCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly UpdateExchangeRateCommandHandler _handler;

        public UpdateExchangeRateCommandHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _handler = new UpdateExchangeRateCommandHandler(_mockContext.Object);
        }

        [Fact]
        public async Task Handle_WithValidData_ShouldUpdateSuccessfully()
        {
            // Arrange
            var rateEntity = new ExchangeRate
            {
                Id = 1,
                FromCurrencyId = 1,
                ToCurrencyId = 2,
                Rate = 1.00m,
                IsActive = true
            };

            // ✅ 这里用 BuildMockDbSet() 而不是 BuildMock()
            var ratesDbSetMock = new List<ExchangeRate> { rateEntity }
                .AsQueryable()
                .BuildMockDbSet();

            _mockContext.Setup(x => x.ExchangeRates).Returns(ratesDbSetMock.Object);
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            var cmd = new UpdateExchangeRateCommand
            {
                Id = 1,
                Rate = 1.25m,
                IsActive = false
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Rate.Should().Be(1.25m);
            result.IsActive.Should().BeFalse();
            rateEntity.LastUpdated.Should().NotBeNull();

            _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRateNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var ratesDbSetMock = new List<ExchangeRate>()
                .AsQueryable()
                .BuildMockDbSet();

            _mockContext.Setup(x => x.ExchangeRates).Returns(ratesDbSetMock.Object);

            var cmd = new UpdateExchangeRateCommand
            {
                Id = 999,
                Rate = 1.15m
            };

            // Act
            var act = async () => await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_ShouldCallSaveChangesOnce()
        {
            // Arrange
            var rateEntity = new ExchangeRate
            {
                Id = 10,
                FromCurrencyId = 1,
                ToCurrencyId = 2,
                Rate = 0.98m,
                IsActive = true
            };

            var ratesDbSetMock = new List<ExchangeRate> { rateEntity }
                .AsQueryable()
                .BuildMockDbSet();

            _mockContext.Setup(x => x.ExchangeRates).Returns(ratesDbSetMock.Object);
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            var cmd = new UpdateExchangeRateCommand
            {
                Id = 10,
                Rate = 1.10m,
                IsActive = false
            };

            // Act
            await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUpdateRateValue()
        {
            // Arrange
            var rateEntity = new ExchangeRate
            {
                Id = 2,
                FromCurrencyId = 1,
                ToCurrencyId = 2,
                Rate = 0.9m,
                IsActive = true
            };

            var ratesDbSetMock = new List<ExchangeRate> { rateEntity }
                .AsQueryable()
                .BuildMockDbSet();

            _mockContext.Setup(x => x.ExchangeRates).Returns(ratesDbSetMock.Object);
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            var cmd = new UpdateExchangeRateCommand
            {
                Id = 2,
                Rate = 1.05m,
                IsActive = true
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.Rate.Should().Be(1.05m);
        }

        [Fact]
        public async Task Handle_ShouldUpdateIsActiveFlag()
        {
            // Arrange
            var rateEntity = new ExchangeRate
            {
                Id = 3,
                FromCurrencyId = 1,
                ToCurrencyId = 2,
                Rate = 1.2m,
                IsActive = true
            };

            var ratesDbSetMock = new List<ExchangeRate> { rateEntity }
                .AsQueryable()
                .BuildMockDbSet();

            _mockContext.Setup(x => x.ExchangeRates).Returns(ratesDbSetMock.Object);
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            var cmd = new UpdateExchangeRateCommand
            {
                Id = 3,
                Rate = 1.25m,
                IsActive = false
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsActive.Should().BeFalse();
        }
    }
}
