namespace Shopbe.Application.Category.Dtos;

public record CategoryQueryDto(
    string? Name,
    Guid? ParentCategoryId
);