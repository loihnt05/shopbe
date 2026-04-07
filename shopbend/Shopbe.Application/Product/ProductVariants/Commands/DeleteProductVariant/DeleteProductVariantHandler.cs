using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Product.ProductVariants.Commands.DeleteProductVariant;

public class DeleteProductVariantHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductVariantCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<bool> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException("Variant id is required.");
        }

        var existing = await _unitOfWork.ProductVariant.GetProductVariantByIdAsync(request.Id);
        if (existing is null)
        {
            return false;
        }

        await _unitOfWork.ProductVariant.DeleteProductVariantAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}