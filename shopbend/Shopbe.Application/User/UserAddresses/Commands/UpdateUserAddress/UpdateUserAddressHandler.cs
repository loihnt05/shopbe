using AutoMapper;
using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.User.UserAddresses.Dtos;

namespace Shopbe.Application.User.UserAddresses.Commands.UpdateUserAddress;

public class UpdateUserAddressHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<UpdateUserAddressCommand, UserAddressResponseDto>
{
    public async Task<UserAddressResponseDto> Handle(UpdateUserAddressCommand request, CancellationToken cancellationToken)
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            throw new UnauthorizedAccessException("Missing Keycloak subject claim.");
        }

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
        if (user is null)
        {
            throw new KeyNotFoundException(
                "User not found in application database. Call POST /api/users/sync first to create/sync the user record.");
        }

        var userAddress = await unitOfWork.UserAddresses.GetUserAddressByIdAsync(request.Id);
        if (userAddress is null)
        {
            throw new KeyNotFoundException($"User address with ID '{request.Id}' not found.");
        }

        if (userAddress.UserId != user.Id)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this address.");
        }

        // Map allowed fields onto existing entity.
        mapper.Map(request.UserAddressRequestDto, userAddress);

        if (userAddress.IsDefault)
        {
            await unitOfWork.UserAddresses.UnsetDefaultForUserAsync(user.Id, exceptAddressId: userAddress.Id);
        }

        await unitOfWork.UserAddresses.UpdateUserAddressAsync(userAddress);
        return mapper.Map<UserAddressResponseDto>(userAddress);
    }
}