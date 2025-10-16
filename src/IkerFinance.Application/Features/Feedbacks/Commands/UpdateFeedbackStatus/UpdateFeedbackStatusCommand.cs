using IkerFinance.Application.DTOs.Feedbacks;
using IkerFinance.Domain.Enums;
using MediatR;

namespace IkerFinance.Application.Features.Feedbacks.Commands.UpdateFeedbackStatus;

public class UpdateFeedbackStatusCommand : IRequest<FeedbackDto>
{
    public int Id { get; set; }
    public string AdminUserId { get; set; } = string.Empty;
    public FeedbackStatus Status { get; set; }
    public string? AdminResponse { get; set; }
}
