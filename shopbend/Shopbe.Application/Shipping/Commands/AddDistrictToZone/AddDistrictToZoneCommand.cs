using MediatR;
using Shopbe.Application.Shipping.Dtos;

namespace Shopbe.Application.Shipping.Commands.AddDistrictToZone;

public sealed record AddDistrictToZoneCommand(Guid ZoneId, string City, string District) : IRequest<ShippingZoneDistrictDto>;




