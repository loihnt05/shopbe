using MediatR;
using Shopbe.Application.Category.Dtos;
using Shopbe.Application.Interfaces;

namespace Shopbe.Application.Category.Queries.GetAllCategories;

public class GetAllCategoriesHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryResponseDto>>
{
    public async Task<IEnumerable<CategoryResponseDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await unitOfWork.Category.GetAllCategoriesAsync();

        return categories
            .Where(c =>
                (request.Filter.Name == null || c.Name.Contains(request.Filter.Name, StringComparison.OrdinalIgnoreCase)) &&
                (request.Filter.ParentCategoryId == null || c.ParentCategoryId == request.Filter.ParentCategoryId))
            .Select(c => new CategoryResponseDto(c.Id, c.Name, c.ParentCategoryId, c.Slug, c.SortOrder, c.IsActive));
    }
}
