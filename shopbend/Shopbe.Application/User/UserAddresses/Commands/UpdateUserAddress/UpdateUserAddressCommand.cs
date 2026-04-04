using MediatR;
using Shopbe.Application.User.UserAddresses.Dtos;

namespace Shopbe.Application.User.UserAddresses.Commands.UpdateUserAddress;

public record UpdateUserAddressCommand(Guid Id, UserAddressRequestDto UserAddressRequestDto) : IRequest<UserAddressResponseDto>;