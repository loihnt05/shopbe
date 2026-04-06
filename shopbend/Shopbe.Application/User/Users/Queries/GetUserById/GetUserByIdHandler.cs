using AutoMapper;
using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Queries.GetUserById;

public class GetUserByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) 
    : IRequestHandler<GetUserByIdQuery, UserResponseDto>
{
    public async Task<UserResponseDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Filter.UserId is null)
        {
            throw new ArgumentException("UserId must be provided.");
        }
        
        var user = await unitOfWork.Users.GetUserByIdAsync(request.Filter.UserId.Value);

        if (user is null)
        {
            throw new KeyNotFoundException($"User with email '{request.Filter.UserId}' not found.");
        }
        
        return mapper.Map<UserResponseDto>(user);
    }
}