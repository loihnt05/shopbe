using MediatR;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Queries.GetAllUsers;

public record GetAllUsersQuery(UserQueryDto Filter) : IRequest<IEnumerable<UserResponseDto>>;
