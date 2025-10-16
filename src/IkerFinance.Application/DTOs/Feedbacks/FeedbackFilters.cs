using IkerFinance.Domain.Enums;

namespace IkerFinance.Application.DTOs.Feedbacks;

public class FeedbackFilters
{
    public string? SearchTerm { get; set; }
    public FeedbackType? Type { get; set; }
    public FeedbackStatus? Status { get; set; }
    public FeedbackPriority? Priority { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public string SortOrder { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
