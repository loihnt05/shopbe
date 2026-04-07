using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Product.ProductImages.Commands.CreateProductImage;

public class CreateProductImageHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductImageCommand, ProductImageResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ProductImageResponseDto> Handle(CreateProductImageCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == Guid.Empty)
        {
            throw new ArgumentException("ProductId is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Request.ImageUrl))
        {
            throw new ArgumentException("ImageUrl is required.");
        }

        var product = await _unitOfWork.Product.GetProductByIdAsync(command.Id);
        if (product is null)
        {
            throw new KeyNotFoundException($"Product with id '{command.Id}' was not found.");
        }

        if (command.Request.IsPrimary)
        {
            var existing = await _unitOfWork.ProductImage.GetProductImagesByProductIdAsync(command.Id);
            if (existing.Any(i => i.IsPrimary))
            {
                throw new ArgumentException("Only one product image can be marked as primary.");
            }
        }

        var image = new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = command.Id,
            ImageUrl = command.Request.ImageUrl.Trim(),
            IsPrimary = command.Request.IsPrimary,
            SortOrder = 0
        };

        await _unitOfWork.ProductImage.AddProductImageAsync(image);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductImageResponseDto(image.Id, image.ImageUrl, image.IsPrimary);
    }
}