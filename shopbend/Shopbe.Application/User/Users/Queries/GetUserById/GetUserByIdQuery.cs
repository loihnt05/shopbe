using MediatR;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Queries.GetUserById;

public record GetUserByIdQuery(UserQueryDto Filter) : IRequest<UserResponseDto>;