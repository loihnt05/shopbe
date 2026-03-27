namespace Shopbe.Domain.Entities.User;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }

    // Navigation properties
    public User? User { get; set; }
}

