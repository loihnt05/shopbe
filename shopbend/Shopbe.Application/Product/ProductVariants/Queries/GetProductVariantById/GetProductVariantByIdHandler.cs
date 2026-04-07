using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.ProductVariants.Queries.GetProductVariantById;

public class GetProductVariantByIdHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetProductVariantByIdQuery, ProductVariantResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ProductVariantResponseDto> Handle(GetProductVariantByIdQuery request, CancellationToken cancellationToken)
    {
        var variant = await _unitOfWork.ProductVariant.GetProductVariantByIdWithAttributesAsync(request.Id);
        if (variant is null)
        {
            throw new KeyNotFoundException($"Variant with id '{request.Id}' was not found.");
        }

        var attributeValueIds = variant.ProductVariantAttributes
            .Select(pva => pva.AttributeValueId)
            .Distinct()
            .ToList();

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