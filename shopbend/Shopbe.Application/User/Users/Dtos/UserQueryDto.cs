namespace Shopbe.Application.User.Users.Dtos;

public class UserQueryDto
{
    public Guid? UserId { get; set; }
    public string? KeycloakId { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}