namespace Shopbe.Application.Category.Dtos;

public record CategoryRequestDto (
    string Name,
    Guid? ParentCategoryId,
    string? Slug,
    int SortOrder,
    bool IsActive
);