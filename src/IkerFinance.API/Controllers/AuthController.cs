using Microsoft.AspNetCore.Mvc;
using MediatR;
using IkerFinance.Shared.DTOs.Auth;
using IkerFinance.Application.Features.Auth.Commands.Login;
using IkerFinance.Application.Features.Auth.Commands.Register;

namespace IkerFinance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterCommand command)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", command.Email);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("User registered successfully: {Email}", command.Email);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Registration failed with validation error for email: {Email}", command.Email);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during registration for email: {Email}", command.Email);
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginCommand command)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", command.Email);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("User logged in successfully: {Email}", command.Email);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login failed - unauthorized access for email: {Email}", command.Email);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during login for email: {Email}", command.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }
}