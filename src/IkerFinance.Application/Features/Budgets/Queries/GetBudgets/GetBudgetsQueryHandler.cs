using MediatR;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Common;
using IkerFinance.Application.DTOs.Budgets;

namespace IkerFinance.Application.Features.Budgets.Queries.GetBudgets;

public sealed class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, PaginatedResponse<BudgetDto>>
{
    private readonly IBudgetRepository _budgetRepository;

    public GetBudgetsQueryHandler(IBudgetRepository budgetRepository)
    {
        _budgetRepository = budgetRepository;
    }

    public async Task<PaginatedResponse<BudgetDto>> Handle(
        GetBudgetsQuery request,
        CancellationToken cancellationToken)
    {
        request.ValidatePagination();

        var filters = new BudgetFilters
        {
            UserId = request.UserId,
            SearchTerm = request.GetNormalizedSearchTerm(),
            IsActive = request.IsActive,
            Period = request.Period,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            SortBy = request.SortBy,
            SortOrder = request.SortOrder,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return await _budgetRepository.GetBudgetsWithDetailsAsync(filters, cancellationToken);
    }
}
