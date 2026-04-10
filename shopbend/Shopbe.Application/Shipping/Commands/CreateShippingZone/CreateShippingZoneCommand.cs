using MediatR;
using Shopbe.Application.Shipping.Dtos;
using Shopbe.Domain.Entities.Shipping;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.IShipping;

namespace Shopbe.Application.Shipping.Commands.CreateShippingZone;

public sealed record CreateShippingZoneCommand(string Name, decimal Fee) : IRequest<ShippingZoneDto>;






