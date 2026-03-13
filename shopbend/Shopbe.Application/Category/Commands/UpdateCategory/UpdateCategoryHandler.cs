using MediatR;
using Shopbe.Application.Category.Dtos;
using Shopbe.Application.Interfaces;

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

        category.Name = request.Request.Name;
        category.ParentCategoryId = request.Request.ParentCategoryId;

        await _unitOfWork.Category.UpdateCategoryAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryResponseDto(category.Id, category.Name, category.ParentCategoryId);
    }
}
