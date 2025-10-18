using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using IkerFinance.Application.Features.ExchangeRates.Commands.CreateExchangeRate;
using IkerFinance.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;
using IkerFinance.Application.Features.ExchangeRates.Commands.DeleteExchangeRate;
using IkerFinance.Application.Features.ExchangeRates.Queries.GetExchangeRates;
using IkerFinance.Application.Features.ExchangeRates.Queries.GetExchangeRateById;

namespace IkerFinance.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ExchangeRatesController> _logger;

    public ExchangeRatesController(IMediator mediator, ILogger<ExchangeRatesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all exchange rates with pagination and filtering (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetExchangeRates([FromQuery] GetExchangeRatesQuery query)
    {
        _logger.LogInformation("Getting exchange rates");

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get exchange rate by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetExchangeRateById(int id)
    {
        _logger.LogInformation("Getting exchange rate with ID: {Id}", id);

        var query = new GetExchangeRateByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create new exchange rate (Admin only)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateExchangeRate([FromBody] CreateExchangeRateCommand command)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        command.AdminUserId = adminUserId!;

        _logger.LogInformation("Creating exchange rate from currency {FromCurrencyId} to {ToCurrencyId} by admin: {AdminUserId}",
            command.FromCurrencyId, command.ToCurrencyId, adminUserId);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetExchangeRateById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update exchange rate (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExchangeRate(int id, [FromBody] UpdateExchangeRateCommand command)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        command.Id = id;
        command.AdminUserId = adminUserId!;

        _logger.LogInformation("Updating exchange rate ID: {Id} by admin: {AdminUserId}", id, adminUserId);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete exchange rate (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExchangeRate(int id)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _logger.LogInformation("Deleting exchange rate ID: {Id} by admin: {AdminUserId}", id, adminUserId);

        var command = new DeleteExchangeRateCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
