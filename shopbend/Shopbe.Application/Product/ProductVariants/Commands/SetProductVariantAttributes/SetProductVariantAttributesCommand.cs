using MediatR;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.ProductVariants.Commands.SetProductVariantAttributes;

public record SetProductVariantAttributesCommand(Guid VariantId, SetProductVariantAttributesRequestDto Request)
    : IRequest<ProductVariantResponseDto>;

