using AutoMapper;
using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.User.UserAddresses.Dtos;

namespace Shopbe.Application.User.UserAddresses.Queries.GetUserAddressById;

public class GetUserAddressByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetUserAddressByIdQuery, UserAddressResponseDto>
{
    public async Task<UserAddressResponseDto> Handle(GetUserAddressByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userAddress = await unitOfWork.UserAddresses.GetUserAddressByIdAsync(request.Filter.UserId);
        if (userAddress == null)
        {
            throw new KeyNotFoundException($"User address with ID '{request.Filter.UserId}' not found.");
        }
        return mapper.Map<UserAddressResponseDto>(userAddress);
    }
    
    
}