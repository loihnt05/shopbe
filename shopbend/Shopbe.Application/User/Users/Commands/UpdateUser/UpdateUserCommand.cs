using MediatR;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Commands.UpdateUser;

public record UpdateUserCommand(Guid Id, UserRequestDto Request) : IRequest<UserResponseDto>;
