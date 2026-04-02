using MediatR;
using Shopbe.Application.Product.Products.Dtos;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Queries.GetAllUsers;

public record GetAllUsersQuery(UserQueryDto Filter) : IRequest<IEnumerable<ProductResponseDto>>, IRequest<IEnumerable<UserResponseDto>>;