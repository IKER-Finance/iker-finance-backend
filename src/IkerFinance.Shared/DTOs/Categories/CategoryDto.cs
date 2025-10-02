using IkerFinance.Domain.Enums;

namespace IkerFinance.Shared.DTOs.Categories;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
}