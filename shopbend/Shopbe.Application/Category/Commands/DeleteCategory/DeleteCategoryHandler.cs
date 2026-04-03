using MediatR;
using Shopbe.Application.Interfaces;

namespace Shopbe.Application.Category.Commands.DeleteCategory;

public class DeleteCategoryHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand, bool>
{
    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await unitOfWork.Category.GetCategoryByIdAsync(request.Id);
        if (category is null)
            throw new KeyNotFoundException($"Category with id '{request.Id}' was not found.");

        await unitOfWork.Category.DeleteCategoryAsync(request.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
