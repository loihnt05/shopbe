using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Product.ProductAttributes.Commands.DeleteProductAttribute;

public class DeleteProductAttributeHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProductAttributeCommand, bool>
{
    public async Task<bool> Handle(DeleteProductAttributeCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException("Attribute id is required.");
        }

        var existing = await unitOfWork.ProductAttribute.GetAttributeByIdAsync(request.Id);
        if (existing is null)
        {
            return false;
        }

        await unitOfWork.ProductAttribute.DeleteAttributeAsync(request.Id);
        return true;
    }
}

