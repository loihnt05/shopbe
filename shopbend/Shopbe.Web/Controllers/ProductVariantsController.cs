using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Product.ProductVariants.Commands.CreateProductVariant;
using Shopbe.Application.Product.ProductVariants.Commands.DeleteProductVariant;
using Shopbe.Application.Product.ProductVariants.Commands.SetProductVariantAttributes;
using Shopbe.Application.Product.ProductVariants.Commands.UpdateProductVariant;
using Shopbe.Application.Product.ProductVariants.Dtos;
using Shopbe.Application.Product.ProductVariants.Queries.GetProductVariantById;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api")]
public class ProductVariantsController(IMediator mediator) : ControllerBase
{
    [HttpGet("variants/{id:guid}")]
    public async Task<ActionResult<ProductVariantResponseDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductVariantByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("products/{productId:guid}/variants")]
    public async Task<ActionResult<ProductVariantResponseDto>> Create(
        [FromRoute] Guid productId,
        [FromBody] ProductVariantRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateProductVariantCommand(request, productId), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("variants/{id:guid}")]
    public async Task<ActionResult<ProductVariantResponseDto>> Update(
        [FromRoute] Guid id,
        [FromBody] ProductVariantRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateProductVariantCommand(request, id), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("variants/{id:guid}/attributes")]
    public async Task<ActionResult<ProductVariantResponseDto>> SetAttributes(
        [FromRoute] Guid id,
        [FromBody] SetProductVariantAttributesRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SetProductVariantAttributesCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("variants/{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteProductVariantCommand(id), cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}

