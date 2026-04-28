using Shopbe.Domain.Enums;

namespace Shopbe.Domain.Entities.Recommendation;
public class UserBehavior : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }

    /// <summary>
    /// Strongly-typed behavior for recommendation queries/scoring.
    /// </summary>
    public BehaviorType BehaviorType { get; set; } = BehaviorType.Unknown;

    /// <summary>
    /// Backward-compatible "string" action. Prefer <see cref="BehaviorType"/>.
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    public Guid? ProductId { get; set; }

    /// <summary>
    /// The UTC instant when the event happened. Defaults to <see cref="BaseEntity.CreatedAt"/>.
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public int? Quantity { get; set; }
    public decimal? Value { get; set; }

    public string? Referrer { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }

    /// <summary>
    /// JSON string for additional properties (search query, page, campaign, etc.).
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation properties
    public User.User? User { get; set; }
    public Product.Product? Product { get; set; }
}

