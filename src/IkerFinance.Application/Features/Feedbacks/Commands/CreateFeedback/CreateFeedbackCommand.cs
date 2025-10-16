using IkerFinance.Application.DTOs.Feedbacks;
using IkerFinance.Domain.Enums;
using MediatR;

namespace IkerFinance.Application.Features.Feedbacks.Commands.CreateFeedback;

public class CreateFeedbackCommand : IRequest<FeedbackDto>
{
    public string UserId { get; set; } = string.Empty;
    public FeedbackType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FeedbackPriority Priority { get; set; } = FeedbackPriority.Medium;
}
