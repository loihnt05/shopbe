using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductVariants.Dtos;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Product.ProductVariants.Commands.CreateProductVariant;

public class CreateProductVariantHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductVariantCommand, ProductVariantResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ProductVariantResponseDto> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty)
        {
            throw new ArgumentException("ProductId is required.");
        }

        var dto = request.Request;

        if (string.IsNullOrWhiteSpace(dto.SKU))
        {
            throw new ArgumentException("SKU is required.");
        }

        if (dto.Price < 0)
        {
            throw new ArgumentException("Variant price cannot be negative.");
        }

        if (dto.StockQuantity < 0)
        {
            throw new ArgumentException("Variant stock quantity cannot be negative.");
        }

        var product = await _unitOfWork.Product.GetProductByIdAsync(request.ProductId);
        if (product is null)
        {
            throw new KeyNotFoundException($"Product with id '{request.ProductId}' was not found.");
        }

        var sku = dto.SKU.Trim();
        var skuExists = await _unitOfWork.ProductVariant.ProductVariantSkuExistsAsync(request.ProductId, sku);
        if (skuExists)
        {
            throw new ArgumentException("Variant SKU must be unique within the product.");
        }

        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Sku = sku,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            IsActive = dto.IsActive,
            DeletedAt = null
        };

        var attributeValueIds = (dto.AttributeValueIds ?? Array.Empty<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (attributeValueIds.Count > 0)
        {
            // Validate attribute value ids exist.
            foreach (var attributeValueId in attributeValueIds)
            {
                var av = await _unitOfWork.AttributeValue.GetValueByIdAsync(attributeValueId);
                if (av is null)
                {
                    throw new KeyNotFoundException($"Attribute value with id '{attributeValueId}' was not found.");
                }

                variant.ProductVariantAttributes.Add(new ProductVariantAttribute
                {
                    VariantId = variant.Id,
                    AttributeValueId = attributeValueId
                });
            }
        }

        await _unitOfWork.ProductVariant.AddProductVariantAsync(variant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductVariantResponseDto(
            variant.Id,
            variant.ProductId,
            variant.Sku,
            variant.Price,
            variant.StockQuantity,
            variant.IsActive,
            attributeValueIds
        );
    }
}