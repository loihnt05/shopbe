using AutoMapper;
using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.User.UserAddresses.Dtos;

namespace Shopbe.Application.User.UserAddresses.Queries.GetAllUserAddresses;

public class GetAllUserAddressesHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetAllUserAddressesQuery, IEnumerable<UserAddressResponseDto>>
{
    public async Task<IEnumerable<UserAddressResponseDto>> Handle(GetAllUserAddressesQuery request,
        CancellationToken cancellationToken)
    {
        var userAddresses = await unitOfWork.UserAddresses.GetAllUserAddressAsync();

        var filter = request.Filter ?? new UserAddressQueryDto();
        
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;
        pageSize = Math.Min(pageSize, 100);

        var query = userAddresses.AsQueryable();
        if (filter.UserId != Guid.Empty)
        {
            query = query.Where(ua => ua.UserId == filter.UserId);
        }

        var pageUserAddress = query
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return mapper.Map<List<UserAddressResponseDto>>(pageUserAddress);
    }
}