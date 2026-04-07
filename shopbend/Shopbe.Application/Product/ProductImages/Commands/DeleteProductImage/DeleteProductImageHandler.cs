using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Product.ProductImages.Commands.DeleteProductImage;

public class DeleteProductImageHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductImageCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<bool> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
    if (request.Id == Guid.Empty)
    {
      throw new ArgumentException("Image id is required.");
    }

    var existing = await _unitOfWork.ProductImage.GetProductImageByIdAsync(request.Id);
    if (existing is null)
    {
      return false;
    }

    await _unitOfWork.ProductImage.DeleteProductImageAsync(request.Id);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return true;
    }           
}