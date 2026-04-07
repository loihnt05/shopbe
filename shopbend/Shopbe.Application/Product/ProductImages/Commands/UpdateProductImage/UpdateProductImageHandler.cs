using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductImages.Dtos;

namespace Shopbe.Application.Product.ProductImages.Commands.UpdateProductImage;

public class UpdateProductImageHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductImageCommand, ProductImageResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ProductImageResponseDto> Handle(UpdateProductImageCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException("Image id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Request.ImageUrl))
        {
            throw new ArgumentException("ImageUrl is required.");
        }

        var image = await _unitOfWork.ProductImage.GetProductImageByIdAsync(request.Id);
        if (image is null)
        {
            throw new KeyNotFoundException($"Product image with id '{request.Id}' was not found.");
        }

        if (request.Request.IsPrimary && !image.IsPrimary)
        {
            var existing = await _unitOfWork.ProductImage.GetProductImagesByProductIdAsync(image.ProductId);
            if (existing.Any(i => i.IsPrimary && i.Id != image.Id))
            {
                throw new ArgumentException("Only one product image can be marked as primary.");
            }
        }

        image.ImageUrl = request.Request.ImageUrl.Trim();
        image.IsPrimary = request.Request.IsPrimary;

        await _unitOfWork.ProductImage.UpdateProductImageAsync(image);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductImageResponseDto(image.Id, image.ImageUrl, image.IsPrimary);
    }
}