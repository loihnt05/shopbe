using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Shipping.Commands.CreateShipment;
using Shopbe.Application.Shipping.Commands.UpdateShipment;
using Shopbe.Application.Shipping.Dtos;
using Shopbe.Application.Shipping.Queries.GetShipmentsByOrderId;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class ShipmentsController(IMediator mediator) : ControllerBase
{
    // Customer can view shipments related to a specific order id that they own.
    // Ownership is not enforced here yet because Order ownership logic is in OrdersController; keep endpoint admin-only by default.
    [HttpGet("orders/{orderId:guid}/shipments")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListByOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetShipmentsByOrderIdQuery(orderId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("shipments")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateShipmentRequestDto request, CancellationToken cancellationToken)
    {
        var created = await mediator.Send(new CreateShipmentCommand(request), cancellationToken);
        return CreatedAtAction(nameof(ListByOrder), new { orderId = created.OrderId }, created);
    }

    [HttpPatch("shipments/{shipmentId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid shipmentId, [FromBody] UpdateShipmentRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await mediator.Send(new UpdateShipmentCommand(shipmentId, request), cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}

