using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Seller;
using Shopbe.Application.Seller.Dtos;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/seller/orders")]
[Authorize(Roles = "Seller")]
public sealed class SellerOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SellerOrderQueryDto query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSellerOrdersQuery(query), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSellerOrderByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] SellerUpdateOrderStatusRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSellerOrderStatusCommand(id, request), cancellationToken);
        return Ok(result);
    }
}
