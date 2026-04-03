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
        var category = new DomainCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Request.Name,
            ParentCategoryId = request.Request.ParentCategoryId,
        };

        await unitOfWork.Category.AddCategoryAsync(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryResponseDto(category.Id, category.Name, category.ParentCategoryId);
    }
}