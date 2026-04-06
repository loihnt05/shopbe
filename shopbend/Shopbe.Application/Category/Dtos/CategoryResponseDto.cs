namespace Shopbe.Application.Category.Dtos;

public record CategoryResponseDto(
    Guid Id,
    string Name,
    Guid? ParentCategoryId,
    string Slug,
    int SortOrder,
    bool IsActive
);