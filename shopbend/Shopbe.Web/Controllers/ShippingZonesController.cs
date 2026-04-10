using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Shipping.Commands.AddDistrictToZone;
using Shopbe.Application.Shipping.Commands.CreateShippingZone;
using Shopbe.Application.Shipping.Commands.RemoveDistrictFromZone;
using Shopbe.Application.Shipping.Queries.GetShippingZoneById;
using Shopbe.Application.Shipping.Queries.GetShippingZones;
using Shopbe.Application.Shipping.Queries.ResolveZoneByAddress;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/shipping-zones")]
public sealed class ShippingZonesController(IMediator mediator) : ControllerBase
{
    // Public lookup endpoints (used by frontend checkout/address selection)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] bool includeDistricts = false, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetShippingZonesQuery(includeDistricts), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] bool includeDistricts = true,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetShippingZoneByIdQuery(id, includeDistricts), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("resolve")]
    [AllowAnonymous]
    public async Task<IActionResult> Resolve([FromQuery] string city, [FromQuery] string district,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ResolveZoneByAddressQuery(city, district), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // Admin endpoints
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateShippingZoneCommand request, CancellationToken cancellationToken)
    {
        var created = await mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    public sealed class AddDistrictRequest
    {
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
    }

    [HttpPost("{zoneId:guid}/districts")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddDistrict(Guid zoneId, [FromBody] AddDistrictRequest request,
        CancellationToken cancellationToken)
    {
        var created = await mediator.Send(new AddDistrictToZoneCommand(zoneId, request.City, request.District), cancellationToken);
        return Ok(created);
    }

    [HttpDelete("{zoneId:guid}/districts/{districtId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveDistrict(Guid zoneId, Guid districtId, CancellationToken cancellationToken)
    {
        var removed = await mediator.Send(new RemoveDistrictFromZoneCommand(zoneId, districtId), cancellationToken);
        return removed ? NoContent() : NotFound();
    }
}

