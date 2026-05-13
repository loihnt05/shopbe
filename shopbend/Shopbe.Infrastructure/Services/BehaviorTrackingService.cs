using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common;
using Shopbe.Domain.Entities.Recommendation;
using Shopbe.Domain.Enums;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Services;

public sealed class BehaviorTrackingService(ShopDbContext db) : IBehaviorTrackingService
{
    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength ? value : value[..maxLength];
    }

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

        var occurredAtUtc = occurredAt ?? DateTime.UtcNow;
        var expiresAtUtc = BehaviorRetentionPolicy.ComputeExpiresAtUtc(behaviorType, occurredAtUtc);

        var entity = new UserBehavior
        {
            UserId = userId,
            SessionId = Truncate(sessionId, 128),
            CorrelationId = Truncate(correlationId, 128),
            BehaviorType = behaviorType,
            ActionType = Truncate(actionType, 100) ?? string.Empty,
            ProductId = productId,
            CategoryId = categoryId,
            OrderId = orderId,
            Quantity = quantity,
            Value = value,
            Currency = Truncate(currency, 3),
            Source = Truncate(source, 64),
            Device = Truncate(device, 64),
            Referrer = Truncate(referrer, 2048),
            UserAgent = Truncate(userAgent, 512),
            IpAddress = Truncate(ipAddress, 64),
            Country = Truncate(country, 2),
            City = Truncate(city, 128),
            Metadata = metadata,
            OccurredAt = occurredAtUtc,
            ExpiresAt = expiresAtUtc
        };

        db.UserBehaviors.Add(entity);
        await db.SaveChangesAsync(ct);
    }
}

