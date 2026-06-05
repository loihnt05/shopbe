using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Admin;
using Shopbe.Application.Admin.Dtos;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public sealed class AdminUsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdminUserQueryDto query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminUsersQuery(query), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminUserByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] AdminUpdateUserStatusRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAdminUserStatusCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] AdminUpdateUserRoleRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAdminUserRoleCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteAdminUserCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
