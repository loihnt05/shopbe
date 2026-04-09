using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Product.Products.Commands.CreateProduct;
using Shopbe.Application.Product.Products.Commands.DeleteProduct;
using Shopbe.Application.Product.Products.Commands.UpdateProduct;
using Shopbe.Application.Product.Products.Dtos;
using Shopbe.Application.Product.Products.Queries.GetAllProducts;
using Shopbe.Application.Product.Products.Queries.GetProductById;

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
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryDto filter, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllProductsQuery(filter), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] ProductRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateProductCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateProductCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
} 