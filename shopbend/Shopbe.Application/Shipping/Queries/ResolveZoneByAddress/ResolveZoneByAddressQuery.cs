using MediatR;
using Shopbe.Application.Shipping.Dtos;

namespace Shopbe.Application.Shipping.Queries.ResolveZoneByAddress;

public sealed record ResolveZoneByAddressQuery(string City, string District) : IRequest<ShippingZoneDto?>;
