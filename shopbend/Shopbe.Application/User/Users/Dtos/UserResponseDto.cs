using Shopbe.Domain.Enums;

namespace Shopbe.Application.User.Users.Dtos;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string KeycloakId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }

    public UserRole? Role { get; set; }
    public UserStatus? Status { get; set; }
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}