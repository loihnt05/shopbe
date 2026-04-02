using MediatR;
using Shopbe.Application.User.UserAddresses.Dtos;

namespace Shopbe.Application.User.UserAddresses.Queries.GetUserAddressById;

public record GetUserAddressByIdQuery(UserAddressQueryDto Filter) : IRequest<UserAddressResponseDto>;