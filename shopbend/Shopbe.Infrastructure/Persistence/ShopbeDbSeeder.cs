using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Product;
using Shopbe.Domain.Entities.Shipping;

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
        await SeedShippingLocationsAsync(db, logger, ct);

        // Only seed catalog if we don't have any products yet.
        if (await db.Products.AsNoTracking().AnyAsync(ct))
        {
            logger?.LogInformation("Catalog seeder skipped: products already exist.");
            return;
        }

        logger?.LogInformation("Seeding sample catalog data...");

        var catFashion = new Category
        {
            Name = "Fashion",
            Slug = "fashion",
            SortOrder = 1,
            IsActive = true
        };

        var catElectronics = new Category
        {
            Name = "Electronics",
            Slug = "electronics",
            SortOrder = 2,
            IsActive = true
        };

        db.Categories.AddRange(catFashion, catElectronics);
        await db.SaveChangesAsync(ct);

        // Product 1
        var tshirt = new Product
        {
            Name = "Basic T-Shirt",
            Slug = "basic-tshirt",
            Description = "Soft cotton T-Shirt",
            BasePrice = 99000m,
            CategoryId = catFashion.Id,
            IsActive = true
        };
        db.Products.Add(tshirt);
        await db.SaveChangesAsync(ct);

        db.ProductImages.Add(new ProductImage
        {
            ProductId = tshirt.Id,
            ImageUrl = "https://picsum.photos/seed/basic-tshirt/800/800",
            AltText = "Basic T-Shirt",
            IsPrimary = true,
            SortOrder = 0
        });

        db.ProductVariants.AddRange(
            new ProductVariant { ProductId = tshirt.Id, Sku = "TSHIRT-S-BLK", Price = 99000m, StockQuantity = 50, IsActive = true },
            new ProductVariant { ProductId = tshirt.Id, Sku = "TSHIRT-M-BLK", Price = 99000m, StockQuantity = 50, IsActive = true },
            new ProductVariant { ProductId = tshirt.Id, Sku = "TSHIRT-L-BLK", Price = 99000m, StockQuantity = 50, IsActive = true }
        );

        // Product 2
        var headphones = new Product
        {
            Name = "Wireless Headphones",
            Slug = "wireless-headphones",
            Description = "Bluetooth over-ear headphones",
            BasePrice = 499000m,
            CategoryId = catElectronics.Id,
            IsActive = true
        };
        db.Products.Add(headphones);
        await db.SaveChangesAsync(ct);

        db.ProductImages.Add(new ProductImage
        {
            ProductId = headphones.Id,
            ImageUrl = "https://picsum.photos/seed/wireless-headphones/800/800",
            AltText = "Wireless Headphones",
            IsPrimary = true,
            SortOrder = 0
        });

        db.ProductVariants.Add(new ProductVariant
        {
            ProductId = headphones.Id,
            Sku = "HPHONES-BLK",
            Price = 499000m,
            StockQuantity = 25,
            IsActive = true
        });

        // Product 3
        var mug = new Product
        {
            Name = "Coffee Mug",
            Slug = "coffee-mug",
            Description = "Ceramic mug 350ml",
            BasePrice = 59000m,
            CategoryId = catFashion.Id,
            IsActive = true
        };
        db.Products.Add(mug);
        await db.SaveChangesAsync(ct);

        db.ProductImages.Add(new ProductImage
        {
            ProductId = mug.Id,
            ImageUrl = "https://picsum.photos/seed/coffee-mug/800/800",
            AltText = "Coffee Mug",
            IsPrimary = true,
            SortOrder = 0
        });

        db.ProductVariants.Add(new ProductVariant
        {
            ProductId = mug.Id,
            Sku = "MUG-350",
            Price = 59000m,
            StockQuantity = 100,
            IsActive = true
        });

        await db.SaveChangesAsync(ct);

        logger?.LogInformation("Seeded sample catalog: {Count} products.", await db.Products.CountAsync(ct));
    }

    private static async Task SeedShippingLocationsAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        // Seed a minimal set of shipping zones + city/district mappings for local testing.
        // Idempotent by unique keys (zone name, and (ZoneId, City, District)).

        // Create or load zones
        var desiredZones = new (string Name, decimal Fee)[]
        {
            ("HCMC", 25000m),
            ("Hanoi", 30000m),
            ("Other", 40000m)
        };

        var zoneNames = desiredZones.Select(z => z.Name.Trim()).ToArray();
        var existingZones = await db.ShippingZones
            .Where(z => zoneNames.Contains(z.Name))
            .ToListAsync(ct);

        var zonesByName = existingZones.ToDictionary(z => z.Name, StringComparer.Ordinal);
        var createdZones = 0;

        foreach (var (nameRaw, fee) in desiredZones)
        {
            var name = nameRaw.Trim();
            if (zonesByName.TryGetValue(name, out var existing))
            {
                // Keep fee in sync for dev seed convenience.
                if (existing.Fee != fee)
                {
                    existing.Fee = fee;
                }

                continue;
            }

            var zone = new ShippingZone { Name = name, Fee = fee };
            db.ShippingZones.Add(zone);
            zonesByName[name] = zone;
            createdZones++;
        }

        if (createdZones > 0)
        {
            await db.SaveChangesAsync(ct);
        }

        // Create district mappings
        // Note: ResolveZoneByAddress trims strings but doesn't normalize casing; keep exact casing here.
        var desiredMappings = new (string ZoneName, string City, string District)[]
        {
            ("HCMC", "Ho Chi Minh", "District 1"),
            ("HCMC", "Ho Chi Minh", "District 3"),
            ("HCMC", "Ho Chi Minh", "Binh Thanh"),
            ("Hanoi", "Ha Noi", "Ba Dinh"),
            ("Hanoi", "Ha Noi", "Cau Giay"),
            ("Other", "Da Nang", "Hai Chau")
        };

        var createdMappings = 0;
        foreach (var (zoneNameRaw, cityRaw, districtRaw) in desiredMappings)
        {
            var zoneName = zoneNameRaw.Trim();
            var city = cityRaw.Trim();
            var district = districtRaw.Trim();

            if (!zonesByName.TryGetValue(zoneName, out var zone))
            {
                // Should not happen, but skip gracefully.
                logger?.LogWarning("Shipping seed: missing zone '{ZoneName}' for mapping {City}/{District}.", zoneName, city, district);
                continue;
            }

            var exists = await db.ShippingZoneDistricts.AsNoTracking().AnyAsync(d =>
                d.ZoneId == zone.Id && d.City == city && d.District == district, ct);

            if (exists)
            {
                continue;
            }

            db.ShippingZoneDistricts.Add(new ShippingZoneDistrict
            {
                ZoneId = zone.Id,
                City = city,
                District = district
            });
            createdMappings++;
        }

        if (createdZones > 0 || createdMappings > 0)
        {
            await db.SaveChangesAsync(ct);
            logger?.LogInformation(
                "Seeded shipping locations: +{Zones} zones, +{Mappings} city/district mappings (total zones: {TotalZones}, total mappings: {TotalMappings}).",
                createdZones,
                createdMappings,
                await db.ShippingZones.CountAsync(ct),
                await db.ShippingZoneDistricts.CountAsync(ct));
        }
        else
        {
            logger?.LogInformation("Shipping locations seeder skipped: shipping zones/mappings already exist.");
        }
    }
}

