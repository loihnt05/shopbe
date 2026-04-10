using MediatR;
using Shopbe.Application.Shipping.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Shipping.Queries.GetShippingZones;

public sealed record GetShippingZonesQuery(bool IncludeDistricts = false) : IRequest<IReadOnlyList<ShippingZoneDto>>;




