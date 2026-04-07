using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Product.ProductImages.Commands.CreateProductImage;
using Shopbe.Application.Product.ProductImages.Commands.DeleteProductImage;
using Shopbe.Application.Product.ProductImages.Commands.UpdateProductImage;
using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.ProductImages.Queries.GetAllProductImage;
using Shopbe.Application.Product.ProductImages.Queries.GetProductImageById;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api")]
public class ProductImagesController(IMediator mediator) : ControllerBase
{
    [HttpGet("images")]
    public async Task<ActionResult<IEnumerable<ProductImageResponseDto>>> GetAll(
        [FromQuery] Guid? productId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetAllProductImageQuery(new ProductImageQueryDto(productId, pageNumber, pageSize)),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("images/{id:guid}")]
    public async Task<ActionResult<ProductImageResponseDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductImageByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("products/{productId:guid}/images")]
    public async Task<ActionResult<ProductImageResponseDto>> Create(
        [FromRoute] Guid productId,
        [FromBody] ProductImageRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateProductImageCommand(request, productId), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("images/{id:guid}")]
    public async Task<ActionResult<ProductImageResponseDto>> Update(
        [FromRoute] Guid id,
        [FromBody] ProductImageRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateProductImageCommand(request, id), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("images/{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteProductImageCommand(id), cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}

