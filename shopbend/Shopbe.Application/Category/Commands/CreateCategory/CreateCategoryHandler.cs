using MediatR;
using Shopbe.Application.Category.Dtos;
using Shopbe.Application.Interfaces;
using Shopbe.Domain.Entities;
using DomainCategory = Shopbe.Domain.Entities.Category.Category;

namespace Shopbe.Application.Category.Commands.CreateCategory;

public class CreateCategoryHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryCommand, CategoryResponseDto>
{
    public async Task<CategoryResponseDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Request.Name))
            throw new ArgumentException("Category name is required.");

        var category = new DomainCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Request.Name,
            ParentCategoryId = request.Request.ParentCategoryId,
            Slug = GenerateSlug(request.Request.Slug, request.Request.Name),
            SortOrder = request.Request.SortOrder,
            IsActive = request.Request.IsActive,
        };

        await unitOfWork.Category.AddCategoryAsync(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryResponseDto(category.Id, category.Name, category.ParentCategoryId, category.Slug, category.SortOrder,
            category.IsActive);
    }

    private static string GenerateSlug(string? requestedSlug, string name)
    {
        var source = string.IsNullOrWhiteSpace(requestedSlug) ? name : requestedSlug;
        source = source.Trim().ToLowerInvariant();

        // minimal slugify: keep letters/digits, convert whitespace/underscores to '-', collapse repeated '-'
        var chars = source
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : (char.IsWhiteSpace(ch) || ch == '_' || ch == '-' ? '-' : '\0'))
            .Where(ch => ch != '\0')
            .ToArray();

        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
            slug = slug.Replace("--", "-", StringComparison.Ordinal);

        slug = slug.Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N") : slug;
    }
}