using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Products.Commands.CreateProduct;
using Shopbe.Application.Products.Commands.DeleteProduct;
using Shopbe.Application.Products.Commands.UpdateProduct;
using Shopbe.Application.Products.Dtos;
using Shopbe.Application.Products.Queries.GetAllProducts;
using Shopbe.Application.Products.Queries.GetProductById;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryDto filter, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllProductsQuery(filter), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateProductCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateProductCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
} 