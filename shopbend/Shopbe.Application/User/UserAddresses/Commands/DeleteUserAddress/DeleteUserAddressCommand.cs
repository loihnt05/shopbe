namespace Shopbe.Application.User.UserAddresses.Commands.DeleteUserAddress;

public record DeleteUserAddressCommand(Guid Id) : MediatR.IRequest<bool>;
