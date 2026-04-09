using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.User.Users.Commands.CreateUser;
using Shopbe.Application.User.Users.Dtos;
using Shopbe.Application.User.Users.Queries.GetAllUsers;
using Shopbe.Application.User.Users.Queries.GetUserByEmail;
using Shopbe.Application.User.Users.Queries.GetUserById;
using Shopbe.Application.User.Users.Queries.GetUserByKeycloakId;

namespace Shopbe.Web.Controllers.UserController;

[ApiController]
[Route("api/users")]
public class UserController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Sync the currently authenticated Keycloak user into the application database.
    /// Safe to call multiple times (idempotent by KeycloakId).
    /// </summary>
    [Authorize]
    [HttpPost("sync")]
    public async Task<ActionResult<UserResponseDto>> Sync([FromBody] UserRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateUserCommand(request), cancellationToken);
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers([FromQuery] UserQueryDto filter, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllUsersQuery(filter), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByIdQuery(new UserQueryDto { UserId = id }), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("by-email")]
    public async Task<ActionResult<UserResponseDto>> GetByEmail([FromQuery] string email, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByEmailQuery(new UserQueryDto { Email = email }), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("by-keycloak")]
    public async Task<ActionResult<UserResponseDto>> GetByKeycloakId([FromQuery] string keycloakId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByKeycloakQuery(new UserQueryDto { KeycloakId = keycloakId }), cancellationToken);
        return Ok(result);
    }

}