using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using IkerFinance.Application.Features.Feedbacks.Commands.CreateFeedback;
using IkerFinance.Application.Features.Feedbacks.Commands.UpdateFeedbackStatus;
using IkerFinance.Application.Features.Feedbacks.Queries.GetFeedbacks;

namespace IkerFinance.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FeedbackController> _logger;

    public FeedbackController(IMediator mediator, ILogger<FeedbackController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Submit feedback (Any authenticated user)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        command.UserId = userId!;

        _logger.LogInformation("Creating feedback for user: {UserId}", userId);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetFeedbacks), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get all feedbacks with pagination (Admin only)
    /// NOTE: Currently allows all authenticated users. Implement role-based authorization for production.
    /// To restrict to admin: Add [Authorize(Roles = "Admin")] when roles are implemented
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetFeedbacks([FromQuery] GetFeedbacksQuery query)
    {
        _logger.LogInformation("Getting feedbacks");

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update feedback status (Admin only)
    /// NOTE: Currently allows all authenticated users. Implement role-based authorization for production.
    /// To restrict to admin: Add [Authorize(Roles = "Admin")] when roles are implemented
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateFeedbackStatus(int id, [FromBody] UpdateFeedbackStatusCommand command)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        command.Id = id;
        command.AdminUserId = adminUserId!;

        _logger.LogInformation("Updating feedback status for feedback ID: {FeedbackId} by admin: {AdminUserId}", id, adminUserId);

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
