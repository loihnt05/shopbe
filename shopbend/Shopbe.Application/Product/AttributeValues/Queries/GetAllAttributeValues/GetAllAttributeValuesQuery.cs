namespace Shopbe.Application.Product.AttributeValues.Queries.GetAllAttributeValues;

using MediatR;
using Shopbe.Application.Product.AttributeValues.Dtos;

public record GetAllAttributeValuesQuery(Guid AttributeId) : IRequest<IEnumerable<AttributeValueResponseDto>>;
