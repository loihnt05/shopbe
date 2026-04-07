using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Product.AttributeValues.Commands.DeleteAttributeValue;

public class DeleteAttributeValueHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAttributeValueCommand, bool>
{
    public async Task<bool> Handle(DeleteAttributeValueCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException("Attribute value id is required.");
        }

        var existing = await unitOfWork.AttributeValue.GetValueByIdAsync(request.Id);
        if (existing is null)
        {
            return false;
        }

        await unitOfWork.AttributeValue.DeleteValueAsync(request.Id);
        return true;
    }
}

