using MediatR;
using Shopbe.Application.Category.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Category.Queries.GetCategoryById;

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CategoryResponseDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoryByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryResponseDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Category.GetCategoryByIdAsync(request.Id);
        if (category is null)
            return null;

        return new CategoryResponseDto(category.Id, category.Name, category.ParentCategoryId, category.Slug, category.SortOrder,
            category.IsActive);
    }
}
