using MediatR;
using Shopbe.Application.Interfaces;

namespace Shopbe.Application.Category.Commands.DeleteCategory;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Category.GetCategoryByIdAsync(request.Id);
        if (category is null)
            throw new KeyNotFoundException($"Category with id '{request.Id}' was not found.");

        await _unitOfWork.Category.DeleteCategoryAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
