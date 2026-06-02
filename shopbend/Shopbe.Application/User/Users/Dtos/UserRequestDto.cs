using System.ComponentModel.DataAnnotations;

namespace Shopbe.Application.User.Users.Dtos;

public record UserRequestDto
{
    // Do NOT accept KeycloakId from clients; it should be derived from the authenticated principal (sub claim).

    // [Required] intentionally omitted — CreateUserHandler falls back to token claims
    // if the request body omits these fields (e.g. empty birthday → null).
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [StringLength(2048)]
    [Url]
    public string? AvatarUrl { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    public DateTime? Birthday { get; set; }

    [StringLength(50)]
    public string? Language { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }
}