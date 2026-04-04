using MediatR;
using Shopbe.Application.User.UserAddresses.Dtos;

namespace Shopbe.Application.User.UserAddresses.Commands.CreateUserAddress;

public record CreateUserAddressCommand(UserAddressRequestDto  Request) : IRequest<UserAddressResponseDto>;