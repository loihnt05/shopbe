using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.User.UserAddresses.Commands.CreateUserAddress;
using Shopbe.Application.User.UserAddresses.Commands.UpdateUserAddress;
using Shopbe.Application.User.UserAddresses.Dtos;
using Shopbe.Application.User.UserAddresses.Queries.GetAllUserAddresses;
using Shopbe.Application.User.UserAddresses.Queries.GetAllUserAddressesByUserId;
using Shopbe.Application.User.UserAddresses.Queries.GetUserAddressById;

namespace Shopbe.Web.Controllers.UserController;

[ApiController]
[Route("api/user-addresses")]
public class UserAddressController(IMediator mediator, ICurrentUser currentUser, IUnitOfWork unitOfWork) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<UserAddressResponseDto>> Create([FromBody] UserAddressRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateUserAddressCommand(request), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserAddressResponseDto>> Update([FromRoute] Guid id,
        [FromBody] UserAddressRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateUserAddressCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserAddressResponseDto>>> GetAllUserAddresses(
        [FromQuery] UserAddressQueryDto filter, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllUserAddressesQuery(filter), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<UserAddressResponseDto>>> GetMyAddresses(
        [FromQuery] UserAddressQueryDto? filter,
        CancellationToken cancellationToken)
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            throw new UnauthorizedAccessException("Missing Keycloak subject claim.");
        }

        var user = await unitOfWork.Users.GetUserByKeyCloakIdAsync(keycloakId);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found in application database. Call POST /api/users/sync first.");
        }

        filter ??= new UserAddressQueryDto();
        filter.UserId = user.Id;

        var result = await mediator.Send(new GetAllUserAddressesByUserIdQuery(user.Id, filter), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserAddressResponseDto>> GetUserAddress([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserAddressByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<UserAddressResponseDto>>> GetAllUserAddressesByUserId(
        [FromRoute] Guid userId,
        [FromQuery] UserAddressQueryDto filter,
        CancellationToken cancellationToken)
    {
        // If you want only the *current* user's addresses, don't expose userId in route.
        // We'll add /api/user-addresses/me in a follow-up.
        filter.UserId = userId;

        var result = await mediator.Send(new GetAllUserAddressesByUserIdQuery(userId, filter), cancellationToken);
        return Ok(result);
    }
}