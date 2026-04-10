using MediatR;
using Shopbe.Application.Shipping.Dtos;
using Shopbe.Domain.Entities.Shipping;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Shipping.Commands.CreateShipment;

public sealed record CreateShipmentCommand(CreateShipmentRequestDto Request) : IRequest<ShipmentDto>;





