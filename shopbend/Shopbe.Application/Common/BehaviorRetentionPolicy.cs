using Shopbe.Domain.Enums;

namespace Shopbe.Application.Common;

public static class BehaviorRetentionPolicy
{
    /// <summary>
    /// Retention rules (in days):
    /// - ProductView: 60 days
    /// - Search: 90 days
    /// - Purchase: 365 days
    /// 
    /// For other behaviors, we keep a conservative default of 90 days.
    /// </summary>
    public static int GetRetentionDays(BehaviorType behaviorType)
    {
        return behaviorType switch
        {
            BehaviorType.ProductView => 60,
            BehaviorType.Search => 90,
            BehaviorType.Purchase => 365,
            _ => 90
        };
    }

    public static DateTime ComputeExpiresAtUtc(BehaviorType behaviorType, DateTime occurredAtUtc)
        => occurredAtUtc.AddDays(GetRetentionDays(behaviorType));
}

