using MediatR;
using Shopbe.Application.Product.AttributeValues.Dtos;

namespace Shopbe.Application.Product.AttributeValues.Commands.CreateAttributeValue;

public record CreateAttributeValueCommand(Guid AttributeId, AttributeValueRequestDto Request)
    : IRequest<AttributeValueResponseDto>;

