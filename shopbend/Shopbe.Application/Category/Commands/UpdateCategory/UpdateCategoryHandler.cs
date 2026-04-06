using MediatR;
using Shopbe.Application.Category.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Category.Commands.UpdateCategory;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, CategoryResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryResponseDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Category.GetCategoryByIdAsync(request.Id);
        if (category is null)
            throw new KeyNotFoundException($"Category with id '{request.Id}' was not found.");

        if (string.IsNullOrWhiteSpace(request.Request.Name))
            throw new ArgumentException("Category name is required.");

        category.Name = request.Request.Name;
        category.ParentCategoryId = request.Request.ParentCategoryId;
        category.Slug = GenerateSlug(request.Request.Slug, request.Request.Name);
        category.SortOrder = request.Request.SortOrder;
        category.IsActive = request.Request.IsActive;

        await _unitOfWork.Category.UpdateCategoryAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryResponseDto(category.Id, category.Name, category.ParentCategoryId, category.Slug, category.SortOrder,
            category.IsActive);
    }

    private static string GenerateSlug(string? requestedSlug, string name)
    {
        var source = string.IsNullOrWhiteSpace(requestedSlug) ? name : requestedSlug;
        source = source.Trim().ToLowerInvariant();

        var chars = source
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : (char.IsWhiteSpace(ch) || ch == '_' || ch == '-' ? '-' : '\0'))
            .Where(ch => ch != '\0')
            .ToArray();

        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
            slug = slug.Replace("--", "-", StringComparison.Ordinal);

        return slug.Trim('-');
    }
}
