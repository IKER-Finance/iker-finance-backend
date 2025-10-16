using IkerFinance.Domain.Enums;

namespace IkerFinance.Application.DTOs.Feedbacks;

public class FeedbackDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public FeedbackType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FeedbackPriority Priority { get; set; }
    public FeedbackStatus Status { get; set; }
    public string? AdminResponse { get; set; }
    public string? RespondedByUserId { get; set; }
    public string? RespondedByUserName { get; set; }
    public DateTime? ResponseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
