using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Admin;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public sealed class AdminDashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminDashboardOverviewQuery(), cancellationToken);
        return Ok(result);
    }
}
