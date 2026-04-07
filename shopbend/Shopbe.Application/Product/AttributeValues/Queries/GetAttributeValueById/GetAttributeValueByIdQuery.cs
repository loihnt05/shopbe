using MediatR;
using Shopbe.Application.Product.AttributeValues.Dtos;

namespace Shopbe.Application.Product.AttributeValues.Queries.GetAttributeValueById;

public record GetAttributeValueByIdQuery(Guid Id) : IRequest<AttributeValueResponseDto>;

