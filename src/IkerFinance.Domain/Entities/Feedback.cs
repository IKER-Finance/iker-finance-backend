using IkerFinance.Domain.Common;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Domain.Entities;

public class Feedback : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
    
    public FeedbackType Type { get; set; } = FeedbackType.Bug;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FeedbackPriority Priority { get; set; } = FeedbackPriority.Medium;
    public FeedbackStatus Status { get; set; } = FeedbackStatus.Open;
    
    public string? AdminResponse { get; set; }
    public string? RespondedByUserId { get; set; }
    public virtual ApplicationUser? RespondedByUser { get; set; }
    public DateTime? ResponseDate { get; set; }
}