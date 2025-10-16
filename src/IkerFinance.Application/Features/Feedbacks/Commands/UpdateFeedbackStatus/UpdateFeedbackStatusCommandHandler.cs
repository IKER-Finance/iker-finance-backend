using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Feedbacks;

namespace IkerFinance.Application.Features.Feedbacks.Commands.UpdateFeedbackStatus;

public sealed class UpdateFeedbackStatusCommandHandler : IRequestHandler<UpdateFeedbackStatusCommand, FeedbackDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateFeedbackStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FeedbackDto> Handle(
        UpdateFeedbackStatusCommand request,
        CancellationToken cancellationToken)
    {
        var feedback = await _context.Feedbacks
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (feedback == null)
            throw new NotFoundException("Feedback", request.Id);

        var adminUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.AdminUserId, cancellationToken);

        if (adminUser == null)
            throw new NotFoundException("Admin User", request.AdminUserId);

        feedback.Status = request.Status;
        feedback.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.AdminResponse))
        {
            feedback.AdminResponse = request.AdminResponse;
            feedback.RespondedByUserId = request.AdminUserId;
            feedback.ResponseDate = DateTime.UtcNow;
        }

        _context.Update(feedback);
        await _context.SaveChangesAsync(cancellationToken);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == feedback.UserId, cancellationToken);

        var respondedByUser = feedback.RespondedByUserId != null
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == feedback.RespondedByUserId, cancellationToken)
            : null;

        return new FeedbackDto
        {
            Id = feedback.Id,
            UserId = feedback.UserId,
            UserName = user != null ? user.FirstName + " " + user.LastName : "Unknown",
            UserEmail = user?.Email ?? "Unknown",
            Type = feedback.Type,
            Subject = feedback.Subject,
            Description = feedback.Description,
            Priority = feedback.Priority,
            Status = feedback.Status,
            AdminResponse = feedback.AdminResponse,
            RespondedByUserId = feedback.RespondedByUserId,
            RespondedByUserName = respondedByUser != null ? respondedByUser.FirstName + " " + respondedByUser.LastName : null,
            ResponseDate = feedback.ResponseDate,
            CreatedAt = feedback.CreatedAt,
            UpdatedAt = feedback.UpdatedAt
        };
    }
}
