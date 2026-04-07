using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Product.AttributeValues.Commands.CreateAttributeValue;
using Shopbe.Application.Product.AttributeValues.Commands.DeleteAttributeValue;
using Shopbe.Application.Product.AttributeValues.Commands.UpdateAttributeValue;
using Shopbe.Application.Product.AttributeValues.Dtos;
using Shopbe.Application.Product.AttributeValues.Queries.GetAllAttributeValues;
using Shopbe.Application.Product.AttributeValues.Queries.GetAttributeValueById;
using Shopbe.Application.Product.AttributeValues.Queries.GetAttributeValuesByAttributeId;
using Shopbe.Application.Product.ProductAttributes.Commands.CreateProductAttribute;
using Shopbe.Application.Product.ProductAttributes.Commands.DeleteProductAttribute;
using Shopbe.Application.Product.ProductAttributes.Commands.UpdateProductAttribute;
using Shopbe.Application.Product.ProductAttributes.Dtos;
using Shopbe.Application.Product.ProductAttributes.Queries.GetAllProductAttributes;
using Shopbe.Application.Product.ProductAttributes.Queries.GetProductAttributeById;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api")]
public class ProductAttributesController(IMediator mediator) : ControllerBase
{
    // --- Product Attributes ---

    [HttpGet("attributes")]
    public async Task<ActionResult<IEnumerable<ProductAttributeResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllProductAttributesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("attributes/{id:guid}")]
    public async Task<ActionResult<ProductAttributeResponseDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductAttributeByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("attributes")]
    public async Task<ActionResult<ProductAttributeResponseDto>> Create([FromBody] ProductAttributeRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateProductAttributeCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("attributes/{id:guid}")]
    public async Task<ActionResult<ProductAttributeResponseDto>> Update([FromRoute] Guid id, [FromBody] ProductAttributeRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateProductAttributeCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("attributes/{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteProductAttributeCommand(id), cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    // --- Attribute Values ---

    // Returns all attribute values; can be filtered by attributeId query parameter.
    [HttpGet("values")]
    public async Task<ActionResult<IEnumerable<AttributeValueResponseDto>>> GetAllValues(
        [FromQuery] Guid attributeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllAttributeValuesQuery(attributeId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("attributes/{attributeId:guid}/values")]
    public async Task<ActionResult<IEnumerable<AttributeValueResponseDto>>> GetValuesByAttributeId(
        [FromRoute] Guid attributeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAttributeValuesByAttributeIdQuery(attributeId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("values/{id:guid}")]
    public async Task<ActionResult<AttributeValueResponseDto>> GetValueById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAttributeValueByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("attributes/{attributeId:guid}/values")]
    public async Task<ActionResult<AttributeValueResponseDto>> CreateValue(
        [FromRoute] Guid attributeId,
        [FromBody] AttributeValueRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateAttributeValueCommand(attributeId, request), cancellationToken);
        return CreatedAtAction(nameof(GetValueById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("values/{id:guid}")]
    public async Task<ActionResult<AttributeValueResponseDto>> UpdateValue(
        [FromRoute] Guid id,
        [FromBody] AttributeValueRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAttributeValueCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("values/{id:guid}")]
    public async Task<ActionResult> DeleteValue([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteAttributeValueCommand(id), cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}

