using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Admin;
using Shopbe.Application.Admin.Dtos;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/admin/sellers")]
[Authorize(Roles = "Admin")]
public sealed class AdminSellersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdminSellerQueryDto query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminSellersQuery(query), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminSellerByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] AdminUpdateSellerStatusRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAdminSellerStatusCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/stats")]
    public async Task<IActionResult> GetStats(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminSellerStatsQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
