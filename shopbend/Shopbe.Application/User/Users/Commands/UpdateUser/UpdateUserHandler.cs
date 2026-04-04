using AutoMapper;
using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Commands.UpdateUser;

public class UpdateUserHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<UpdateUserCommand, UserResponseDto>
{
    public async Task<UserResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetUserByIdAsync(request.Id);
        if (user is null)
        {
            throw new KeyNotFoundException($"User with ID {request.Id} not found.");
        }

        // Update profile fields (but not KeycloakId).
        user.Email = request.Request.Email;
        user.FullName = request.Request.FullName;
        user.AvatarUrl = request.Request.AvatarUrl;
        user.PhoneNumber = request.Request.PhoneNumber;

        await unitOfWork.Users.UpdateUserAsync(user);
        return mapper.Map<UserResponseDto>(user);
    }
}