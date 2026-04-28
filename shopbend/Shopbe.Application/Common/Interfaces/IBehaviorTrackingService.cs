using Shopbe.Domain.Enums;

namespace Shopbe.Application.Common.Interfaces;

public interface IBehaviorTrackingService
{
    Task TrackAsync(
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
        CancellationToken ct = default);
}

