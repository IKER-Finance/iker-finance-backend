namespace IkerFinance.Application.Common.Queries;

public abstract class SearchableQuery<TResponse> : PaginatedQuery<TResponse>
{
    public string? SearchTerm { get; set; }
    
    public string? GetNormalizedSearchTerm()
    {
        return string.IsNullOrWhiteSpace(SearchTerm) 
            ? null 
            : SearchTerm.Trim().ToLower();
    }
}