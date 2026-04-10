using MediatR;
using Shopbe.Application.Shipping.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Shipping.Queries.GetShippingZoneById;

public sealed record GetShippingZoneByIdQuery(Guid Id, bool IncludeDistricts = true) : IRequest<ShippingZoneDto?>;




