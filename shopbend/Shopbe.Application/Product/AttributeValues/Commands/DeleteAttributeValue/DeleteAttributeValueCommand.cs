using MediatR;

namespace Shopbe.Application.Product.AttributeValues.Commands.DeleteAttributeValue;

public record DeleteAttributeValueCommand(Guid Id) : IRequest<bool>;

