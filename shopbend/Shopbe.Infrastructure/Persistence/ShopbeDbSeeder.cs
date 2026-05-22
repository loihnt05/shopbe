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

        logger?.LogInformation("Seeding sample catalog data...");

        // Seed from multiple sources. Seeders are idempotent by slug/sku.
        await DummyJsonSeeder.SeedAsync(db, logger, ct: ct);
        await EscuelaSeeder.SeedAsync(db, logger, ct: ct);

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

