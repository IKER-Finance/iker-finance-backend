using MediatR;

namespace IkerFinance.Application.Common.Queries;

public abstract class PaginatedQuery<TResponse> : IRequest<TResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public string SortOrder { get; set; } = "desc";
    
    public void ValidatePagination()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 10;
        if (PageSize > 100) PageSize = 100;
        
        SortOrder = SortOrder?.ToLower() == "asc" ? "asc" : "desc";
    }
}