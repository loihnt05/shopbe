using MediatR;
using Shopbe.Application.Shipping.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Shipping.Commands.UpdateShipment;

public sealed record UpdateShipmentCommand(Guid ShipmentId, UpdateShipmentRequestDto Request) : IRequest<ShipmentDto?>;




