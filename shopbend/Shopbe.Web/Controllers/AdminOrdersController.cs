using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Admin;
using Shopbe.Application.Admin.Dtos;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public sealed class AdminOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdminOrderQueryDto query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminOrdersQuery(query), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminOrderByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
