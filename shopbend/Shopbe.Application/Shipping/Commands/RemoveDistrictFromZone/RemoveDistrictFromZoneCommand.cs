using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Shipping.Commands.RemoveDistrictFromZone;

public sealed record RemoveDistrictFromZoneCommand(Guid ZoneId, Guid DistrictId) : IRequest<bool>;




