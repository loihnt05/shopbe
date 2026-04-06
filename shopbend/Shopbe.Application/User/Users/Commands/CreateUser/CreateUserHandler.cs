using AutoMapper;
using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.User.Users.Dtos;

namespace Shopbe.Application.User.Users.Commands.CreateUser;

public class CreateUserHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<CreateUserCommand, UserResponseDto>
{
    public async Task<UserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Keycloak creates the auth account first; we only *sync* it into our DB.
        // The canonical ID is the `sub` claim.
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            throw new UnauthorizedAccessException(
                "Missing Keycloak subject claim. Expected 'sub' (or NameIdentifier). " +
                "Make sure Swagger/Frontend is sending an access_token from Keycloak with the 'openid' scope.");
        }

        // Prefer claims as source-of-truth, but allow request as fallback for profile fields.
        var email = FirstNonEmpty(currentUser.Email, request.Request.Email);
        var fullName = FirstNonEmpty(currentUser.FullName, request.Request.FullName);

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email must be provided (token email claim or request.Email).");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("FullName must be provided (token name claim or request.FullName).");
        }

        // 1) If user already exists by KeycloakId => update profile fields (sync) and return.
        var existingByKeycloak = await unitOfWork.Users.GetUserByKeyCloakIdAsync(keycloakId);
        if (existingByKeycloak is not null)
        {
            existingByKeycloak.Email = email;
            existingByKeycloak.FullName = fullName;
            existingByKeycloak.AvatarUrl = request.Request.AvatarUrl;
            existingByKeycloak.PhoneNumber = request.Request.PhoneNumber;

            await unitOfWork.Users.UpdateUserAsync(existingByKeycloak);
            return mapper.Map<UserResponseDto>(existingByKeycloak);
        }

        // 2) Prevent silent linking: if email already exists with a different KeycloakId, treat as conflict.
        var existingByEmail = await unitOfWork.Users.GetUserByEmailAsync(email);
        if (existingByEmail is not null)
        {
            throw new InvalidOperationException(
                $"Email '{email}' is already in use by a different account.");
        }

        var newUser = new Domain.Entities.User.User
        {
            KeycloakId = keycloakId,
            Email = email,
            FullName = fullName,
            AvatarUrl = request.Request.AvatarUrl,
            PhoneNumber = request.Request.PhoneNumber
            // Role/Status default values are configured at the persistence layer.
        };

        try
        {
            await unitOfWork.Users.CreateUserAsync(newUser);
        }
        catch (Exception)
        {
            // Concurrency/idempotency: another request might have created it. Re-read and return.
            var created = await unitOfWork.Users.GetUserByKeyCloakIdAsync(keycloakId);
            if (created is null) throw;

            return mapper.Map<UserResponseDto>(created);
        }

        return mapper.Map<UserResponseDto>(newUser);
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}