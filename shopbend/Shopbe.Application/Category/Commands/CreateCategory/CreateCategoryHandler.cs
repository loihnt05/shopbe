using MediatR;
using Shopbe.Application.Category.Dtos;
using Shopbe.Application.Interfaces;
using Shopbe.Domain.Entities;
using DomainCategory = Shopbe.Domain.Entities.Category;

namespace Shopbe.Application.Category.Commands.CreateCategory;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CategoryResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryResponseDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new DomainCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Request.Name,
            ParentCategoryId = request.Request.ParentCategoryId,
        };

        await _unitOfWork.Category.AddCategoryAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryResponseDto(category.Id, category.Name, category.ParentCategoryId);
    }
}