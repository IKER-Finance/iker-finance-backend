using FluentAssertions;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.ExchangeRates.Commands.CreateExchangeRate;
using IkerFinance.Domain.Entities;
using IkerFinance.Application.Common.Identity;
using Moq;
using MockQueryable.Moq;

namespace IkerFinance.UnitTests.Application.Features.ExchangeRates.Commands;

public class CreateExchangeRateCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CreateExchangeRateCommandHandler _handler;

    public CreateExchangeRateCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new CreateExchangeRateCommandHandler(_mockContext.Object);
    }

    // Test 1: Successfully creates exchange rate record
    [Fact]
    public async Task Handle_WithValidData_CreatesExchangeRateSuccessfully()
    {
        // Arrange
        var adminUser = new ApplicationUser { Id = "admin1", FirstName = "John", LastName = "Doe" };
        var fromCurrency = new Currency { Id = 1, Code = "USD", Name = "US Dollar" };
        var toCurrency = new Currency { Id = 2, Code = "EUR", Name = "Euro" };

        var users = new List<ApplicationUser> { adminUser }.AsQueryable().BuildMockDbSet();
        var currencies = new List<Currency> { fromCurrency, toCurrency }.AsQueryable().BuildMockDbSet();
        var exchangeRates = new List<ExchangeRate>().AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(users.Object);
        _mockContext.Setup(x => x.Currencies).Returns(currencies.Object);
        _mockContext.Setup(x => x.ExchangeRates).Returns(exchangeRates.Object);
        _mockContext.Setup(x => x.Add(It.IsAny<ExchangeRate>()));
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateExchangeRateCommand
        {
            AdminUserId = "admin1",
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = 1.25m,
            EffectiveDate = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Rate.Should().Be(1.25m);
        result.FromCurrencyCode.Should().Be("USD");
        result.ToCurrencyCode.Should().Be("EUR");
        result.UpdatedByUserName.Should().Be("John Doe");

        _mockContext.Verify(x => x.Add(It.IsAny<ExchangeRate>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test 2: Throws NotFoundException when AdminUser not found
    [Fact]
    public async Task Handle_WhenAdminUserNotFound_ShouldThrowNotFoundException()
    {
        var emptyUsers = new List<ApplicationUser>().AsQueryable().BuildMockDbSet();
        var currencies = new List<Currency>
        {
            new Currency { Id = 1, Code = "USD" },
            new Currency { Id = 2, Code = "EUR" }
        }.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(emptyUsers.Object);
        _mockContext.Setup(x => x.Currencies).Returns(currencies.Object);

        var command = new CreateExchangeRateCommand
        {
            AdminUserId = "missingUser",
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = 1.2m,
            EffectiveDate = DateTime.UtcNow
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Admin User*");
    }

    // Test 3: Throws NotFoundException when FromCurrency not found
    [Fact]
    public async Task Handle_WhenFromCurrencyNotFound_ShouldThrowNotFoundException()
    {
        var adminUser = new ApplicationUser { Id = "admin1" };
        var users = new List<ApplicationUser> { adminUser }.AsQueryable().BuildMockDbSet();
        var currencies = new List<Currency> { new Currency { Id = 2, Code = "EUR" } }.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(users.Object);
        _mockContext.Setup(x => x.Currencies).Returns(currencies.Object);

        var command = new CreateExchangeRateCommand
        {
            AdminUserId = "admin1",
            FromCurrencyId = 999,
            ToCurrencyId = 2,
            Rate = 1.2m,
            EffectiveDate = DateTime.UtcNow
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*From Currency*");
    }

    // Test 4: Throws NotFoundException when ToCurrency not found
    [Fact]
    public async Task Handle_WhenToCurrencyNotFound_ShouldThrowNotFoundException()
    {
        var adminUser = new ApplicationUser { Id = "admin1" };
        var users = new List<ApplicationUser> { adminUser }.AsQueryable().BuildMockDbSet();
        var currencies = new List<Currency> { new Currency { Id = 1, Code = "USD" } }.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(users.Object);
        _mockContext.Setup(x => x.Currencies).Returns(currencies.Object);

        var command = new CreateExchangeRateCommand
        {
            AdminUserId = "admin1",
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = 1.2m,
            EffectiveDate = DateTime.UtcNow
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*To Currency*");
    }

    // Test 5: Sets ExchangeRate properties correctly
    [Fact]
    public async Task Handle_ShouldSetExchangeRatePropertiesCorrectly()
    {
        var adminUser = new ApplicationUser { Id = "admin1", FirstName = "Jane", LastName = "Smith" };
        var fromCurrency = new Currency { Id = 1, Code = "USD", Name = "US Dollar" };
        var toCurrency = new Currency { Id = 2, Code = "EUR", Name = "Euro" };
        var users = new List<ApplicationUser> { adminUser }.AsQueryable().BuildMockDbSet();
        var currencies = new List<Currency> { fromCurrency, toCurrency }.AsQueryable().BuildMockDbSet();

        Feedback? capturedEntity = null;
        _mockContext.Setup(x => x.Users).Returns(users.Object);
        _mockContext.Setup(x => x.Currencies).Returns(currencies.Object);
        _mockContext.Setup(x => x.Add(It.IsAny<ExchangeRate>())).Callback<object>(e => capturedEntity = e as Feedback);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateExchangeRateCommand
        {
            AdminUserId = "admin1",
            FromCurrencyId = 1,
            ToCurrencyId = 2,
            Rate = 1.18m,
            EffectiveDate = DateTime.UtcNow,
            IsActive = true
        };

        await _handler.Handle(command, CancellationToken.None);

        _mockContext.Verify(x => x.Add(It.IsAny<ExchangeRate>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
