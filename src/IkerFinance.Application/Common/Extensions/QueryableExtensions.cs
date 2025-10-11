using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.DTOs.Common;

namespace IkerFinance.Application.Common.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        string sortBy,
        string sortOrder,
        Dictionary<string, Expression<Func<T, object>>> sortExpressions)
    {
        if (!sortExpressions.ContainsKey(sortBy))
            sortBy = sortExpressions.Keys.First();

        var expression = sortExpressions[sortBy];
        
        return sortOrder == "asc"
            ? query.OrderBy(expression)
            : query.OrderByDescending(expression);
    }

    public static async Task<PaginatedResponse<TDto>> ToPaginatedListAsync<TEntity, TDto>(
        this IQueryable<TEntity> query,
        int pageNumber,
        int pageSize,
        Func<TEntity, TDto> mapper,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<TDto>
        {
            Data = items.Select(mapper).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}