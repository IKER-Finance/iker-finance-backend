using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using IkerFinance.Application.Features.Users.Queries.GetUserProfile;
using IkerFinance.Application.Features.Users.Commands.UpdateUserProfile;
using IkerFinance.Application.Features.Users.Commands.ChangePassword;
using IkerFinance.Application.Features.Users.Commands.UpdateUserSettings;

namespace IkerFinance.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Getting profile for user: {UserId}", userId);

        var query = new GetUserProfileQuery { UserId = userId! };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        command.UserId = userId!;

        _logger.LogInformation("Updating profile for user: {UserId}", userId);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        command.UserId = userId!;

        _logger.LogInformation("Changing password for user: {UserId}", userId);

        var result = await _mediator.Send(command);
        return Ok(new { success = result, message = "Password changed successfully" });
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Getting settings for user: {UserId}", userId);

        var query = new GetUserProfileQuery { UserId = userId! };
        var profile = await _mediator.Send(query);

        return Ok(new
        {
            homeCurrencyId = profile.HomeCurrencyId,
            defaultTransactionCurrencyId = profile.DefaultTransactionCurrencyId,
            timeZone = profile.TimeZone
        });
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateUserSettingsCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        command.UserId = userId!;

        _logger.LogInformation("Updating settings for user: {UserId}", userId);

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
