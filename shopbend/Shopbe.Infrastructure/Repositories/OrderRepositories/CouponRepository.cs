using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IOrder;
using Shopbe.Domain.Entities.Order;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.OrderRepositories;

public sealed class CouponRepository(ShopDbContext context) : ICouponRepository
{
    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var normalized = code.Trim();

        return await context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == normalized, cancellationToken);
    }

    public async Task<bool> TryConsumeAsync(Guid couponId, Guid userId, Guid orderId, DateTime usedAtUtc,
        CancellationToken cancellationToken = default)
    {
        // 1) Atomically increment UsageCount only if within limit.
        //    This prevents concurrent oversubscription.
        var affected = await context.Database.ExecuteSqlInterpolatedAsync($$"""
UPDATE "Coupons"
SET "UsageCount" = "UsageCount" + 1,
    "UpdatedAt" = CURRENT_TIMESTAMP
WHERE "Id" = {{couponId}}
  AND ("UsageLimit" IS NULL OR "UsageCount" < "UsageLimit");
""" , cancellationToken);

        if (affected == 0)
            return false;

        // 2) Record usage row as audit trail.
        var usage = new CouponUsage
        {
            Id = Guid.NewGuid(),
            CouponId = couponId,
            UserId = userId,
            OrderId = orderId,
            UsedAt = usedAtUtc,
            CreatedAt = usedAtUtc,
            UpdatedAt = usedAtUtc
        };

        await context.CouponUsages.AddAsync(usage, cancellationToken);
        return true;
    }
}


