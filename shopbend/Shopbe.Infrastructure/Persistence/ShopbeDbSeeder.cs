using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Entities.Product;
using Shopbe.Domain.Entities.Shipping;
using Shopbe.Domain.Enums;

namespace Shopbe.Infrastructure.Persistence;

/// <summary>
/// Simple development seeder to create a few categories/products/variants so you can test the buying flow manually.
/// Safe to run multiple times (idempotent by slug/sku).
/// </summary>
public static class ShopbeDbSeeder
{
    public static async Task SeedAsync(ShopDbContext db, ILogger? logger = null, CancellationToken ct = default)
    {
        // Seed is split into sections so we can safely add more seed types over time.
        // (e.g. shipping locations) without being blocked by existing catalog data.
        await ShippingDataSeeder.SeedAsync(db, logger, ct);
        await SeedCouponsAsync(db, logger, ct);

        logger?.LogInformation("Seeding sample catalog data...");

        // Seed from multiple sources. Seeders are idempotent by slug/sku.
        await DummyJsonSeeder.SeedAsync(db, logger, ct: ct);
        await EscuelaSeeder.SeedAsync(db, logger, ct: ct);

        await db.SaveChangesAsync(ct);

        logger?.LogInformation("Seeded sample catalog: {Count} products.", await db.Products.CountAsync(ct));
    }

    private static async Task SeedCouponsAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var desiredCoupons = new List<Coupon>
        {
            new()
            {
                Code = "HELLO2026",
                Description = "10% off for new year",
                DiscountType = DiscountType.Percentage,
                Value = 10,
                MinOrderAmount = 0,
                MaxDiscountAmount = 1000000,
                ExpiredAt = DateTime.UtcNow.AddYears(1),
                Count = 1000,
                IsActive = true
            },
            new()
            {
                Code = "FREESHIP",
                Description = "Free shipping for orders over 500k",
                DiscountType = DiscountType.FreeShipping,
                Value = 0,
                MinOrderAmount = 500000,
                ExpiredAt = DateTime.UtcNow.AddYears(1),
                Count = 999999,
                IsActive = true
            },
            new()
            {
                Code = "SAVE50K",
                Description = "50k off for orders over 200k",
                DiscountType = DiscountType.FixedAmount,
                Value = 50000,
                MinOrderAmount = 200000,
                ExpiredAt = DateTime.UtcNow.AddYears(1),
                Count = 999999,
                IsActive = true
            },
            new()
            {
                Code = "BIGSALE",
                Description = "50% off! (Max 200k discount)",
                DiscountType = DiscountType.Percentage,
                Value = 50,
                MinOrderAmount = 100000,
                MaxDiscountAmount = 200000,
                ExpiredAt = DateTime.UtcNow.AddYears(1),
                Count = 500,
                IsActive = true
            },
            new()
            {
                Code = "FLASH20",
                Description = "Limited time 20% off!",
                DiscountType = DiscountType.Percentage,
                Value = 20,
                MinOrderAmount = 0,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                Count = 10,
                IsActive = true
            },
            new()
            {
                Code = "WELCOME100",
                Description = "100k off for big spenders (Min 1M)",
                DiscountType = DiscountType.FixedAmount,
                Value = 100000,
                MinOrderAmount = 1000000,
                ExpiredAt = DateTime.UtcNow.AddYears(1),
                Count = 50,
                IsActive = true
            },
            new()
            {
                Code = "EXHAUSTED",
                Description = "This coupon is all gone",
                DiscountType = DiscountType.Percentage,
                Value = 99,
                MinOrderAmount = 0,
                ExpiredAt = DateTime.UtcNow.AddYears(1),
                Count = 0,
                IsActive = true
            },
            new()
            {
                Code = "EXPIRED",
                Description = "This coupon ended yesterday",
                DiscountType = DiscountType.Percentage,
                Value = 50,
                MinOrderAmount = 0,
                ExpiredAt = DateTime.UtcNow.AddDays(-1),
                Count = 100,
                IsActive = true
            }
        };

        var codes = desiredCoupons.Select(c => c.Code).ToArray();
        var existingCodes = await db.Coupons
            .Where(c => codes.Contains(c.Code))
            .Select(c => c.Code)
            .ToListAsync(ct);

        var created = 0;
        foreach (var coupon in desiredCoupons)
        {
            if (existingCodes.Contains(coupon.Code)) continue;

            db.Coupons.Add(coupon);
            created++;
        }

        if (created > 0)
        {
            await db.SaveChangesAsync(ct);
            logger?.LogInformation("Seeded {Count} sample coupons.", created);
        }
    }
}

