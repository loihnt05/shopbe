using AutoMapper;
using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.User.UserAddresses.Dtos;

namespace Shopbe.Application.User.UserAddresses.Queries.GetUserAddressById;

public class GetUserAddressByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetUserAddressByIdQuery, UserAddressResponseDto>
{
    public async Task<UserAddressResponseDto> Handle(GetUserAddressByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userAddress = await unitOfWork.UserAddresses.GetUserAddressByIdAsync(request.Id);
        if (userAddress == null || userAddress.DeletedAt != null)
        {
            throw new KeyNotFoundException($"User address with ID '{request.Id}' not found.");
        }
        return mapper.Map<UserAddressResponseDto>(userAddress);
    }
    
    
}