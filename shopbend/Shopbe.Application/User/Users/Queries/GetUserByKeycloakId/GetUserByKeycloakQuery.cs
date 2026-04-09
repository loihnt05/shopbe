using MediatR;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Queries.GetUserByKeycloakId;

public record GetUserByKeycloakQuery(UserQueryDto Filter) : IRequest<UserResponseDto>;