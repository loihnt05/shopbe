using AutoMapper;
using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Queries.GetAllUsers;

public class GetAllUsersHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetAllUsersQuery, IEnumerable<UserResponseDto>>
{
    public async Task<IEnumerable<UserResponseDto>> Handle(GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await unitOfWork.Users.GetAllUsersAsync();
        var filter = request.Filter ?? new UserQueryDto();
        
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;
        pageSize = Math.Min(pageSize, 100);

        var query = users.AsQueryable();

        if (filter.UserId.HasValue)
        {
            query = query.Where(u => u.Id == filter.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.KeycloakId))
        {
            query = query.Where(u => u.KeycloakId == filter.KeycloakId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            query = query.Where(u => u.Email.Contains(filter.Email, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.FullName))
        {
            query = query.Where(u => u.FullName.Contains(filter.FullName, StringComparison.OrdinalIgnoreCase));
        }

        var pagedUsers = query
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return mapper.Map<List<UserResponseDto>>(pagedUsers);
    }
}