using MediatR;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Queries.GetUserByEmail;

public record GetUserByEmailQuery(UserQueryDto Filter) : IRequest<UserResponseDto>;