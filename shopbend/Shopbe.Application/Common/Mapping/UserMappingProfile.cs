using AutoMapper;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.Common.Mapping;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // Domain -> DTO
        CreateMap<Shopbe.Domain.Entities.User.User, UserResponseDto>();
    }
}


