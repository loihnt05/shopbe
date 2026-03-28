namespace Shopbe.Domain.Entities.Recommendation;

public class UserBehavior : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public Guid? ProductId { get; set; }
    public string? Metadata { get; set; }

    // Navigation properties
    public User.User? User { get; set; }
    public Product.Product? Product { get; set; }
}

