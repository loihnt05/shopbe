using MediatR;
using Shopbe.Application.Category.Dtos;
using Shopbe.Application.Interfaces;

namespace Shopbe.Application.Category.Queries.GetAllCategories;

public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCategoriesHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CategoryResponseDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Category.GetAllCategoriesAsync();

        return categories
            .Where(c =>
                (request.Filter.Name == null || c.Name.Contains(request.Filter.Name, StringComparison.OrdinalIgnoreCase)) &&
                (request.Filter.ParentCategoryId == null || c.ParentCategoryId == request.Filter.ParentCategoryId))
            .Select(c => new CategoryResponseDto(c.Id, c.Name, c.ParentCategoryId));
    }
}
