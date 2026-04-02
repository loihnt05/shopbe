using MediatR;
using Shopbe.Application.User.UserAddresses.Dtos;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.UserAddresses.Queries.GetAllUserAddresses;

public record GetAllUserAddressesQuery(UserAddressQueryDto Filter) : IRequest<IEnumerable<UserAddressResponseDto>>;