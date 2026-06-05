using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Admin;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
public sealed class AdminAnalyticsController(IMediator mediator) : ControllerBase
{
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] string period = "monthly", CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAdminRevenueQuery(period), cancellationToken);
        return Ok(result);
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSales([FromQuery] string period = "monthly", CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAdminSalesQuery(period), cancellationToken);
        return Ok(result);
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAdminTopProductsQuery(take), cancellationToken);
        return Ok(result);
    }

    [HttpGet("top-sellers")]
    public async Task<IActionResult> GetTopSellers([FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAdminTopSellersQuery(take), cancellationToken);
        return Ok(result);
    }
}
