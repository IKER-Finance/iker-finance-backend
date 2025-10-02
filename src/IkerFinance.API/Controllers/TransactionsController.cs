using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using IkerFinance.Application.Features.Transactions.Commands.CreateTransaction;
using IkerFinance.Application.Features.Transactions.Commands.UpdateTransaction;
using IkerFinance.Application.Features.Transactions.Commands.DeleteTransaction;
using IkerFinance.Application.Features.Transactions.Queries.GetTransactions;
using IkerFinance.Application.Features.Transactions.Queries.GetTransactionById;
using IkerFinance.Application.Features.Transactions.Queries.GetTransactionSummary;

namespace IkerFinance.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(IMediator mediator, ILogger<TransactionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        command.UserId = userId!;
        
        _logger.LogInformation("Creating transaction for user: {UserId}", userId);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTransactionById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] GetTransactionsQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        query.UserId = userId!;
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetTransactionSummary([FromQuery] GetTransactionSummaryQuery query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        query.UserId = userId!;
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = new GetTransactionByIdQuery { Id = id, UserId = userId! };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateTransactionCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        command.Id = id;
        command.UserId = userId!;
        
        _logger.LogInformation("Updating transaction {Id} for user: {UserId}", id, userId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new DeleteTransactionCommand { Id = id, UserId = userId! };
        
        _logger.LogInformation("Deleting transaction {Id} for user: {UserId}", id, userId);
        await _mediator.Send(command);
        return NoContent();
    }
}