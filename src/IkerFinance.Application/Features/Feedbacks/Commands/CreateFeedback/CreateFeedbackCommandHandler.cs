using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Feedbacks;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Application.Features.Feedbacks.Commands.CreateFeedback;

public sealed class CreateFeedbackCommandHandler : IRequestHandler<CreateFeedbackCommand, FeedbackDto>
{
    private readonly IApplicationDbContext _context;

    public CreateFeedbackCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FeedbackDto> Handle(
        CreateFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", request.UserId);

        var feedback = new Feedback
        {
            UserId = request.UserId,
            Type = request.Type,
            Subject = request.Subject,
            Description = request.Description,
            Priority = request.Priority,
            Status = FeedbackStatus.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Add(feedback);
        await _context.SaveChangesAsync(cancellationToken);

        return new FeedbackDto
        {
            Id = feedback.Id,
            UserId = feedback.UserId,
            UserName = user.FirstName + " " + user.LastName,
            UserEmail = user.Email!,
            Type = feedback.Type,
            Subject = feedback.Subject,
            Description = feedback.Description,
            Priority = feedback.Priority,
            Status = feedback.Status,
            AdminResponse = feedback.AdminResponse,
            RespondedByUserId = feedback.RespondedByUserId,
            RespondedByUserName = null,
            ResponseDate = feedback.ResponseDate,
            CreatedAt = feedback.CreatedAt,
            UpdatedAt = feedback.UpdatedAt
        };
    }
}
