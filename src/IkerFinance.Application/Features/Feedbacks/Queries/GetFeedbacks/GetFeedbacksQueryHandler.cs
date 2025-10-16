using MediatR;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.Feedbacks;

namespace IkerFinance.Application.Features.Feedbacks.Queries.GetFeedbacks;

public sealed class GetFeedbacksQueryHandler : IRequestHandler<GetFeedbacksQuery, PaginatedResponse<FeedbackDto>>
{
    private readonly IFeedbackRepository _feedbackRepository;

    public GetFeedbacksQueryHandler(IFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public async Task<PaginatedResponse<FeedbackDto>> Handle(
        GetFeedbacksQuery request,
        CancellationToken cancellationToken)
    {
        request.ValidatePagination();

        var filters = new FeedbackFilters
        {
            SearchTerm = request.GetNormalizedSearchTerm(),
            Type = request.Type,
            Status = request.Status,
            Priority = request.Priority,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            SortBy = request.SortBy,
            SortOrder = request.SortOrder,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return await _feedbackRepository.GetFeedbacksAsync(filters, cancellationToken);
    }
}
