using MediatR;
using IkerFinance.Shared.DTOs.Categories;

namespace IkerFinance.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQuery : IRequest<List<CategoryDto>>
{
    public string UserId { get; set; } = string.Empty;
}