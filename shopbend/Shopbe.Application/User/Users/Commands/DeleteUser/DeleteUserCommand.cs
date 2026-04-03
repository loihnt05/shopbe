using MediatR;

namespace Shopbe.Application.User.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : IRequest<bool>;