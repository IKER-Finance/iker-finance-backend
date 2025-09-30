using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using IkerFinance.Application.Features.Budgets.Commands.CreateBudget;
using IkerFinance.Application.Features.Budgets.Commands.UpdateBudget;
using IkerFinance.Application.Features.Budgets.Commands.DeleteBudget;
using IkerFinance.Application.Features.Budgets.Queries.GetBudgets;
using IkerFinance.Application.Features.Budgets.Queries.GetBudgetById;

namespace IkerFinance.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BudgetsController> _logger;

    public BudgetsController(IMediator mediator, ILogger<BudgetsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        command.UserId = userId!;
        
        _logger.LogInformation("Creating budget for user: {UserId}", userId);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetBudgetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetBudgets([FromQuery] GetBudgetsQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        query.UserId = userId!;
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBudgetById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = new GetBudgetByIdQuery { Id = id, UserId = userId! };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBudget(int id, [FromBody] UpdateBudgetCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        command.Id = id;
        command.UserId = userId!;
        
        _logger.LogInformation("Updating budget {Id} for user: {UserId}", id, userId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBudget(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new DeleteBudgetCommand { Id = id, UserId = userId! };
        
        _logger.LogInformation("Deleting budget {Id} for user: {UserId}", id, userId);
        await _mediator.Send(command);
        return NoContent();
    }
}