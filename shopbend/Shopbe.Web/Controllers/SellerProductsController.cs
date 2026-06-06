using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Seller;
using Shopbe.Application.Seller.Dtos;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/seller/products")]
[Authorize(Roles = "Seller")]
public sealed class SellerProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SellerProductQueryDto query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSellerProductsQuery(query), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SellerProductUpsertDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateSellerProductCommand(request), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SellerProductUpsertDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSellerProductCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteSellerProductCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
