using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Category.Commands.CreateCategory;
using Shopbe.Application.Category.Commands.DeleteCategory;
using Shopbe.Application.Category.Commands.UpdateCategory;
using Shopbe.Application.Category.Dtos;
using Shopbe.Application.Category.Queries.GetAllCategories;
using Shopbe.Application.Category.Queries.GetCategoryById;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] CategoryQueryDto filter, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllCategoriesQuery(filter), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CategoryRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateCategoryCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateCategoryCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}