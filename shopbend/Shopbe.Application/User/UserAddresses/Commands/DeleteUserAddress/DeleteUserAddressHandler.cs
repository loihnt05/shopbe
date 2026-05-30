using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.User.UserAddresses.Commands.DeleteUserAddress;

public class DeleteUserAddressHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<DeleteUserAddressCommand, bool>
{
    public async Task<bool> Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            throw new UnauthorizedAccessException("Missing user identity.");
        }

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var address = await unitOfWork.UserAddresses.GetUserAddressByIdAsync(request.Id);
        if (address is null)
        {
            return false;
        }

        if (address.UserId != user.Id)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this address.");
        }

        await unitOfWork.UserAddresses.DeleteUserAddressAsync(address);
        return true;
    }
}
