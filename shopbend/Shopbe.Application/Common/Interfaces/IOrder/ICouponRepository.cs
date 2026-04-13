using CouponEntity = Shopbe.Domain.Entities.Order.Coupon;

namespace Shopbe.Application.Common.Interfaces.IOrder;

public interface ICouponRepository
{
    Task<IEnumerable<CouponEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<CouponEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CouponEntity?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CouponEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(string code, Guid? excludingCouponId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(CouponEntity coupon, CancellationToken cancellationToken = default);

    Task UpdateAsync(CouponEntity coupon, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically increments coupon usage count (when within limit) and records a CouponUsage row.
    /// Returns true if the coupon was consumed; false if usage limit would be exceeded.
    /// </summary>
    Task<bool> TryConsumeAsync(Guid couponId, Guid userId, Guid orderId, DateTime usedAtUtc,
        CancellationToken cancellationToken = default);
}

