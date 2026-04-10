using MediatR;
using Shopbe.Application.Shipping.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Shipping.Queries.GetShipmentsByOrderId;

public sealed record GetShipmentsByOrderIdQuery(Guid OrderId) : IRequest<IReadOnlyList<ShipmentDto>>;




