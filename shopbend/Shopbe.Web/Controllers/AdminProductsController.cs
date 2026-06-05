using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Admin;
using Shopbe.Application.Admin.Dtos;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public sealed class AdminProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdminProductQueryDto query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminProductsQuery(query), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/approval")]
    public async Task<IActionResult> UpdateApproval(Guid id, [FromBody] AdminUpdateProductApprovalRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAdminProductApprovalCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/visibility")]
    public async Task<IActionResult> UpdateVisibility(Guid id, [FromBody] AdminUpdateProductVisibilityRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAdminProductVisibilityCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteAdminProductCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
