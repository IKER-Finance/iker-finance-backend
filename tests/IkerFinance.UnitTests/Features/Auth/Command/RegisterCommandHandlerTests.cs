using FluentAssertions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Features.Auth.Commands.Register;
using IkerFinance.Application.Common.Identity;
using IkerFinance.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace IkerFinance.UnitTests.Application.Features.Auth.Commands;

public class RegisterCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _mockUserManager = MockUserManager();
        _mockTokenService = new Mock<ITokenService>();
        _handler = new RegisterCommandHandler(_mockUserManager.Object, _mockTokenService.Object);
    }

    // Test: Handler successfully creates user and returns auth response
    [Fact]
    public async Task Handle_WithValidData_CreatesUserAndReturnsAuthResponse()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "John",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        var expectedToken = "test-jwt-token";

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(expectedToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken);
        result.User.Email.Should().Be(command.Email);
        result.User.FirstName.Should().Be(command.FirstName);
        result.User.LastName.Should().Be(command.LastName);
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromSeconds(5));
    }

    // Test: Sets user properties correctly during registration
    [Fact]
    public async Task Handle_SetsUserPropertiesCorrectly()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "John",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        ApplicationUser? capturedUser = null;

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, _) => capturedUser = user)
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token");

        await _handler.Handle(command, CancellationToken.None);

        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(command.Email);
        capturedUser.UserName.Should().Be(command.Email);
        capturedUser.FirstName.Should().Be(command.FirstName);
        capturedUser.LastName.Should().Be(command.LastName);
        capturedUser.IsActive.Should().BeTrue();
        capturedUser.RegistrationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // Test: Throws exception when user creation fails
    [Fact]
    public async Task Handle_WhenUserCreationFails_ThrowsApplicationException()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "John",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        var errors = new[]
        {
            new IdentityError { Code = "DuplicateUserName", Description = "Username already exists" },
            new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" }
        };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("*Registration failed*");
    }

    // Test: Exception message includes all error descriptions
    [Fact]
    public async Task Handle_WhenMultipleErrors_IncludesAllErrorsInException()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "weak",
            ConfirmPassword: "weak",
            FirstName: "John",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        var errors = new[]
        {
            new IdentityError { Description = "Password too short" },
            new IdentityError { Description = "Password requires digit" }
        };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ApplicationException>();
        exception.Which.Message.Should().Contain("Password too short");
        exception.Which.Message.Should().Contain("Password requires digit");
    }

    // Test: Calls token service with correct user
    [Fact]
    public async Task Handle_CallsTokenServiceWithCorrectUser()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "John",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        ApplicationUser? tokenUser = null;

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>()))
            .Callback<ApplicationUser>(user => tokenUser = user)
            .ReturnsAsync("token");

        await _handler.Handle(command, CancellationToken.None);

        tokenUser.Should().NotBeNull();
        tokenUser!.Email.Should().Be(command.Email);
        _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<ApplicationUser>()), Times.Once);
    }

    // Test: RefreshToken is empty in initial implementation
    [Fact]
    public async Task Handle_ReturnsEmptyRefreshToken()
    {
        var command = new RegisterCommand(
            Email: "test@example.com",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            FirstName: "John",
            LastName: "Doe",
            HomeCurrencyId: 1
        );

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.RefreshToken.Should().BeEmpty();
    }

    // Helper method to create UserManager mock
    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }
}