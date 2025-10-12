using IkerFinance.Application.DTOs.Budgets;
using IkerFinance.Application.DTOs.Common;

namespace IkerFinance.Application.Common.Interfaces;

public interface IBudgetRepository
{
    Task<PaginatedResponse<BudgetDto>> GetBudgetsWithDetailsAsync(
        BudgetFilters filters,
        CancellationToken cancellationToken = default);

    Task<BudgetDto?> GetBudgetWithDetailsAsync(
        int id,
        string userId,
        CancellationToken cancellationToken = default);
}
