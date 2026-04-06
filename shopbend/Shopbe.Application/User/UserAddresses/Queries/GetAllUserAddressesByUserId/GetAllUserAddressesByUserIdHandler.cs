using AutoMapper;
using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.User.UserAddresses.Dtos;

namespace Shopbe.Application.User.UserAddresses.Queries.GetAllUserAddressesByUserId;

public class GetAllUserAddressesByUserIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetAllUserAddressesByUserIdQuery, IEnumerable<UserAddressResponseDto>>
{
    public async Task<IEnumerable<UserAddressResponseDto>> Handle(GetAllUserAddressesByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        // IMPORTANT:
        // Repository already scopes by userId; don't materialize early then re-query in-memory.
        // Keep filter/paging logic centralized and (when possible) performed server-side.
        var filter = request.Filter;

        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;
        pageSize = Math.Min(pageSize, 100);

        // Always enforce the route userId.
        var effectiveUserId = request.Id;
        if (effectiveUserId == Guid.Empty)
        {
            return Array.Empty<UserAddressResponseDto>();
        }

        // Fetch scoped data then apply additional filters.
        var userAddresses = await unitOfWork.UserAddresses.GetUserAddressesByUserIdAsync(effectiveUserId);

        var query = userAddresses
            .Where(ua => ua.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            query = query.Where(ua => ua.City.Contains(filter.City));
        }

        if (!string.IsNullOrWhiteSpace(filter.District))
        {
            query = query.Where(ua => ua.District.Contains(filter.District));
        }

        if (!string.IsNullOrWhiteSpace(filter.Ward))
        {
            query = query.Where(ua => ua.Ward.Contains(filter.Ward));
        }

        if (filter.IsDefault.HasValue)
        {
            query = query.Where(ua => ua.IsDefault == filter.IsDefault.Value);
        }

        var pageUserAddress = query
            .OrderByDescending(u => u.IsDefault)
            .ThenBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return mapper.Map<List<UserAddressResponseDto>>(pageUserAddress);
    }
    
    
}