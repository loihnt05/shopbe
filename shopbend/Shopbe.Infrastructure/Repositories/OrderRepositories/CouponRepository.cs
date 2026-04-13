using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IOrder;
using Shopbe.Domain.Entities.Order;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.OrderRepositories;

public sealed class CouponRepository(ShopDbContext context) : ICouponRepository
{
    public async Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Coupons
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Coupon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Coupon?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Coupons
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var normalized = code.Trim();

        return await context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == normalized, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludingCouponId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
        var normalized = code.Trim();

        var query = context.Coupons.AsQueryable();
        if (excludingCouponId.HasValue)
        {
            query = query.Where(c => c.Id != excludingCouponId.Value);
        }

        return await query.AnyAsync(c => c.Code == normalized, cancellationToken);
    }

    public async Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default)
    {
        await context.Coupons.AddAsync(coupon, cancellationToken);
    }

    public Task UpdateAsync(Coupon coupon, CancellationToken cancellationToken = default)
    {
        context.Coupons.Update(coupon);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var coupon = await context.Coupons.FindAsync([id], cancellationToken);
        if (coupon != null)
        {
            context.Coupons.Remove(coupon);
        }
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
                                                                            """, cancellationToken);

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