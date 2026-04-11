using Shopbe.Domain.Entities.Order;

namespace Shopbe.Application.Common.Interfaces.IOrder;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically increments coupon usage count (when within limit) and records a CouponUsage row.
    /// Returns true if the coupon was consumed; false if usage limit would be exceeded.
    /// </summary>
    Task<bool> TryConsumeAsync(Guid couponId, Guid userId, Guid orderId, DateTime usedAtUtc,
        CancellationToken cancellationToken = default);
}

