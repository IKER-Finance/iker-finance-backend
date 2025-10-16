using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.Feedbacks;

namespace IkerFinance.Application.Common.Interfaces;

public interface IFeedbackRepository
{
    Task<PaginatedResponse<FeedbackDto>> GetFeedbacksAsync(
        FeedbackFilters filters,
        CancellationToken cancellationToken = default);
}
