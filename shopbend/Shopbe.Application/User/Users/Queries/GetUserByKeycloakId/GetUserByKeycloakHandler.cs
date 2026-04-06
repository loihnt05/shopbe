using AutoMapper;
using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Queries.GetUserByKeyCloakId;

public class GetUserByKeyCloakId(IUnitOfWork unitOfWork, IMapper mapper) 
    : IRequestHandler<GetUserByKeycloakQuery, UserResponseDto>
{
    public async Task<UserResponseDto> Handle(GetUserByKeycloakQuery request, CancellationToken cancellationToken)
    {
        if (request.Filter.KeycloakId is null)
        {
            throw new ArgumentException("Keycloak must be provided.");
        }
        
        var user = await unitOfWork.Users.GetUserByKeyCloakIdAsync(request.Filter.KeycloakId);

        if (user is null)
        {
            throw new KeyNotFoundException($"User with email '{request.Filter.KeycloakId}' not found.");
        }
        
        return mapper.Map<UserResponseDto>(user);
    }
}