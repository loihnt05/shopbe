using AutoMapper;
using Shopbe.Application.User.UserAddresses.Dtos;

namespace Shopbe.Application.Common.Mapping;

public class UserAddressMappingProfile : Profile
{
    public UserAddressMappingProfile()
    {
        // Domain -> DTO
        CreateMap<Shopbe.Domain.Entities.User.UserAddress, UserAddressResponseDto>();

        // DTO -> Domain
        CreateMap<UserAddressRequestDto, Shopbe.Domain.Entities.User.UserAddress>();
    }
}

