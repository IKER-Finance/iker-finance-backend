using IkerFinance.Application.Common.Queries;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.Feedbacks;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Application.Features.Feedbacks.Queries.GetFeedbacks;

public class GetFeedbacksQuery : SearchableQuery<PaginatedResponse<FeedbackDto>>
{
    public FeedbackType? Type { get; set; }
    public FeedbackStatus? Status { get; set; }
    public FeedbackPriority? Priority { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
