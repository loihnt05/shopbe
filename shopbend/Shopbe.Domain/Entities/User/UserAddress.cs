namespace Shopbe.Domain.Entities.User;

public class UserAddress : BaseEntity
{
    public Guid UserId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    public User? User { get; set; }
}