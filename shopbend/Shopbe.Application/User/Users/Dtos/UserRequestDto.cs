using System.ComponentModel.DataAnnotations;

namespace Shopbe.Application.User.Users.Dtos;

public record UserRequestDto
{
    // Do NOT accept KeycloakId from clients; it should be derived from the authenticated principal (sub claim).

    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [StringLength(2048)]
    [Url]
    public string? AvatarUrl { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
}