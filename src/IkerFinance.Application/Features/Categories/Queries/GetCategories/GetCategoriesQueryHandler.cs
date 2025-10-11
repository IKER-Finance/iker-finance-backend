using MediatR;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.DTOs.Categories;
using IkerFinance.Domain.Entities;

namespace IkerFinance.Application.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IReadRepository<Category> _categoryRepository;

    public GetCategoriesQueryHandler(IReadRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.FindAsync(
            c => c.IsActive && (c.IsSystem || c.UserId == request.UserId),
            cancellationToken);

        return categories
            .OrderBy(c => c.Type)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Color = c.Color,
                Icon = c.Icon,
                Type = c.Type,
                IsSystem = c.IsSystem,
                IsActive = c.IsActive
            })
            .ToList();
    }
}