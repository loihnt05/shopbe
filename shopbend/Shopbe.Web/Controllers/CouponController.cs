using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Coupon.Commands.CreateCoupon;
using Shopbe.Application.Coupon.Commands.DeleteCoupon;
using Shopbe.Application.Coupon.Commands.UpdateCoupon;
using Shopbe.Application.Coupon.Dtos;
using Shopbe.Application.Coupon.Queries.GetAllCoupons;
using Shopbe.Application.Coupon.Queries.GetCouponByCode;
using Shopbe.Application.Coupon.Queries.GetCouponById;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/coupons")]
public class CouponController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] CouponQueryDto filter, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllCouponsQuery(filter), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCouponByIdQuery(id), cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("by-code/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCouponByCodeQuery(code), cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CouponRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateCouponCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] CouponRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateCouponCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCouponCommand(id), cancellationToken);
        return NoContent();
    }
}