namespace Shopbe.Application.Product.ProductVariants.Dtos;

public record SetProductVariantAttributesRequestDto(IReadOnlyCollection<Guid>? AttributeValueIds);

