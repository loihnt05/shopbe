using AutoMapper;
using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.User.UserAddresses.Dtos;

namespace Shopbe.Application.User.UserAddresses.Commands.CreateUserAddress;

public class CreateUserAddressHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<CreateUserAddressCommand, UserAddressResponseDto>
{
    public async Task<UserAddressResponseDto> Handle(CreateUserAddressCommand request, CancellationToken cancellationToken)
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            throw new UnauthorizedAccessException(
                "Missing Keycloak subject claim. Expected 'sub' (or NameIdentifier). " +
                "Make sure the client sends a valid access_token issued by Keycloak.");
        }

        var user = await unitOfWork.Users.GetUserByKeyCloakIdAsync(keycloakId);
        if (user is null)
        {
            throw new KeyNotFoundException(
                "User not found in application database. Call POST /api/users/sync first to create/sync the user record.");
        }

        var userAddress = mapper.Map<Domain.Entities.User.UserAddress>(request.Request);
        userAddress.UserId = user.Id;

        if (userAddress.IsDefault)
        {
            await unitOfWork.UserAddresses.UnsetDefaultForUserAsync(user.Id);
        }

        await unitOfWork.UserAddresses.CreateUserAddressAsync(userAddress);
        return mapper.Map<UserAddressResponseDto>(userAddress);
    }
}