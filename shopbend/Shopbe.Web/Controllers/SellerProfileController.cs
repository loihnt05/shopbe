using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Seller;
using Shopbe.Application.Seller.Dtos;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/seller/profile")]
[Authorize(Roles = "Seller")]
public sealed class SellerProfileController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSellerProfileQuery(), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] SellerProfileUpsertDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSellerProfileCommand(request), cancellationToken);
        return Ok(result);
    }
}
