using MediatR;
using Shopbe.Application.User.UserAddresses.Dtos;

namespace Shopbe.Application.User.UserAddresses.Queries.GetAllUserAddressesByUserId;

public record GetAllUserAddressesByUserIdQuery(Guid Id, UserAddressQueryDto Filter) : IRequest<IEnumerable<UserAddressResponseDto>>;