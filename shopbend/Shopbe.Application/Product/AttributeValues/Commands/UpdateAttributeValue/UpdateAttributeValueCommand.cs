using MediatR;
using Shopbe.Application.Product.AttributeValues.Dtos;

namespace Shopbe.Application.Product.AttributeValues.Commands.UpdateAttributeValue;

public record UpdateAttributeValueCommand(Guid Id, AttributeValueRequestDto Request)
    : IRequest<AttributeValueResponseDto>;

