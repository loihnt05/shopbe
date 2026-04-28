using Shopbe.Application.Common.Interfaces;
using Shopbe.Domain.Entities.Recommendation;
using Shopbe.Domain.Enums;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Services;

public sealed class BehaviorTrackingService(ShopDbContext db) : IBehaviorTrackingService
{
    public async Task TrackAsync(
        Guid? userId,
        string? sessionId,
        string? correlationId,
        BehaviorType behaviorType,
        string actionType,
        Guid? productId = null,
        Guid? categoryId = null,
        Guid? orderId = null,
        int? quantity = null,
        decimal? value = null,
        string? currency = null,
        string? source = null,
        string? device = null,
        string? referrer = null,
        string? userAgent = null,
        string? ipAddress = null,
        string? country = null,
        string? city = null,
        string? metadata = null,
        DateTime? occurredAt = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(actionType))
            actionType = behaviorType.ToString();

        var entity = new UserBehavior
        {
            UserId = userId,
            SessionId = sessionId,
            CorrelationId = correlationId,
            BehaviorType = behaviorType,
            ActionType = actionType,
            ProductId = productId,
            CategoryId = categoryId,
            OrderId = orderId,
            Quantity = quantity,
            Value = value,
            Currency = currency,
            Source = source,
            Device = device,
            Referrer = referrer,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            Country = country,
            City = city,
            Metadata = metadata,
            OccurredAt = occurredAt ?? DateTime.UtcNow
        };

        db.UserBehaviors.Add(entity);
        await db.SaveChangesAsync(ct);
    }
}

