using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Seller;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/seller/dashboard")]
[Authorize(Roles = "Seller")]
public sealed class SellerDashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSellerDashboardOverviewQuery(), cancellationToken);
        return Ok(result);
    }
}
