using MediatR;
using Shopbe.Application.Product.AttributeValues.Dtos;

namespace Shopbe.Application.Product.AttributeValues.Queries.GetAttributeValuesByAttributeId;

public record GetAttributeValuesByAttributeIdQuery(Guid AttributeId) : IRequest<IEnumerable<AttributeValueResponseDto>>;

