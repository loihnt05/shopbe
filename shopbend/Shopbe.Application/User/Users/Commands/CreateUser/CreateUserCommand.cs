using MediatR;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Commands.CreateUser;

public record CreateUserCommand(UserRequestDto Request) : IRequest<UserResponseDto>;