using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductVariants.Dtos;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Product.ProductVariants.Commands.UpdateProductVariant;

public class UpdateProductVariantHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductVariantCommand, ProductVariantResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ProductVariantResponseDto> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException("Variant id is required.");
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

        var variant = await _unitOfWork.ProductVariant.GetProductVariantByIdWithAttributesAsync(request.Id);
        if (variant is null)
        {
            throw new KeyNotFoundException($"Variant with id '{request.Id}' was not found.");
        }

        var sku = dto.SKU.Trim();
        var skuExists = await _unitOfWork.ProductVariant.ProductVariantSkuExistsAsync(variant.ProductId, sku, excludingVariantId: variant.Id);
        if (skuExists)
        {
            throw new ArgumentException("Variant SKU must be unique within the product.");
        }

        variant.Sku = sku;
        variant.Price = dto.Price;
        variant.StockQuantity = dto.StockQuantity;
        variant.IsActive = dto.IsActive;

        var attributeValueIds = (dto.AttributeValueIds ?? Array.Empty<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        // Replace attributes collection
        variant.ProductVariantAttributes.Clear();
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

        await _unitOfWork.ProductVariant.UpdateProductVariantAsync(variant);
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