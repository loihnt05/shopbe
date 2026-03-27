namespace Shopbe.Domain.Entities.Recommendation;

public class UserBehavior : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? ProductId { get; set; }
    public string BehaviorType { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }

    // Navigation properties
    public User.User? User { get; set; }
    public Product.Product? Product { get; set; }
}

