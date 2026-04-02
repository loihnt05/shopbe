using AutoMapper;
using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Queries.GetUserByEmail;

public class GetUserByEmailHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetUserByEmailQuery, UserResponseDto>
{
    public async Task<UserResponseDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        if (request.Filter.Email is null)
        {
            throw new ArgumentException("Email must be provided.");
        }
        var user = await unitOfWork.Users.GetUserByEmailAsync(request.Filter.Email);

        if (user is null)
        {
            throw new KeyNotFoundException($"User with email '{request.Filter.Email}' not found.");
        }

        return mapper.Map<UserResponseDto>(user);
    }
}