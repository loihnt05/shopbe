using MediatR;
using Shopbe.Application.Interfaces;

namespace Shopbe.Application.Product.Products.Commands.DeleteProduct;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Product.GetProductByIdAsync(request.Id);
        if (product is null)
            throw new KeyNotFoundException($"Product with id '{request.Id}' was not found.");

        await _unitOfWork.Product.DeleteProductAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
