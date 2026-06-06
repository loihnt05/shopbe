using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Seller;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/seller/analytics")]
[Authorize(Roles = "Seller")]
public sealed class SellerAnalyticsController(IMediator mediator) : ControllerBase
{
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] string period = "monthly", CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetSellerRevenueQuery(period), cancellationToken);
        return Ok(result);
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSales([FromQuery] string period = "monthly", CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetSellerSalesQuery(period), cancellationToken);
        return Ok(result);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetSellerLowStockProductsQuery(threshold), cancellationToken);
        return Ok(result);
    }
}
