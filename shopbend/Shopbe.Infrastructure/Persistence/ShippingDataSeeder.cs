using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopbe.Domain.Entities.Shipping;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shopbe.Infrastructure.Persistence;

public static class ShippingDataSeeder
{
    public static async Task SeedAsync(ShopDbContext db, ILogger? logger = null, CancellationToken ct = default)
    {
        await SeedShippingZonesAsync(db, logger, ct);
        await SeedShippingConfigsAsync(db, logger, ct);
    }

    private static async Task SeedShippingZonesAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var desiredZones = new (string Name, decimal Fee)[]
        {
            ("HCMC Inner", 15000m),
            ("HCMC Suburbs", 25000m),
            ("National Inner", 35000m),
            ("National Remote", 50000m)
        };

        var zoneNames = desiredZones.Select(z => z.Name).ToArray();
        var existingZones = await db.ShippingZones
            .Where(z => zoneNames.Contains(z.Name))
            .ToListAsync(ct);

        var zonesByName = existingZones.ToDictionary(z => z.Name, StringComparer.Ordinal);
        var createdZones = 0;

        foreach (var (name, fee) in desiredZones)
        {
            if (zonesByName.TryGetValue(name, out var existing))
            {
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

        if (createdZones > 0 || db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(ct);
        }

        var desiredMappings = new (string ZoneName, string City, string District)[]
        {
            // HCMC Inner
            ("HCMC Inner", "Hồ Chí Minh", "Quận 1"),
            ("HCMC Inner", "Hồ Chí Minh", "Quận 3"),
            ("HCMC Inner", "Hồ Chí Minh", "Quận 4"),
            ("HCMC Inner", "Hồ Chí Minh", "Quận 5"),
            ("HCMC Inner", "Hồ Chí Minh", "Quận 10"),
            ("HCMC Inner", "Hồ Chí Minh", "Quận 11"),
            ("HCMC Inner", "Hồ Chí Minh", "Quận Phú Nhuận"),
            ("HCMC Inner", "Hồ Chí Minh", "Quận Bình Thạnh"),
            
            // HCMC Suburbs
            ("HCMC Suburbs", "Hồ Chí Minh", "Quận 7"),
            ("HCMC Suburbs", "Hồ Chí Minh", "Quận 8"),
            ("HCMC Suburbs", "Hồ Chí Minh", "Quận 12"),
            ("HCMC Suburbs", "Hồ Chí Minh", "Huyện Nhà Bè"),
            ("HCMC Suburbs", "Hồ Chí Minh", "Huyện Hóc Môn"),
            ("HCMC Suburbs", "Hồ Chí Minh", "Huyện Bình Chánh"),
            
            // National Inner
            ("National Inner", "Hà Nội", "Quận Ba Đình"),
            ("National Inner", "Hà Nội", "Quận Hoàn Kiếm"),
            ("National Inner", "Hà Nội", "Quận Tây Hồ"),
            ("National Inner", "Đà Nẵng", "Quận Hải Châu"),
            ("National Inner", "Đà Nẵng", "Quận Thanh Khê"),
            
            // National Remote
            ("National Remote", "Lâm Đồng", "Huyện Lạc Dương"),
            ("National Remote", "Hà Giang", "Huyện Đồng Văn")
        };

        var createdMappings = 0;
        foreach (var (zoneName, city, district) in desiredMappings)
        {
            if (!zonesByName.TryGetValue(zoneName, out var zone)) continue;

            var exists = await db.ShippingZoneDistricts.AnyAsync(d =>
                d.ZoneId == zone.Id && d.City == city && d.District == district, ct);

            if (exists) continue;

            db.ShippingZoneDistricts.Add(new ShippingZoneDistrict
            {
                ZoneId = zone.Id,
                City = city,
                District = district
            });
            createdMappings++;
        }

        if (createdMappings > 0)
        {
            await db.SaveChangesAsync(ct);
            logger?.LogInformation("Seeded {Count} shipping mappings.", createdMappings);
        }
    }

    private static async Task SeedShippingConfigsAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var desiredConfigs = new Dictionary<string, decimal>
        {
            { "CONFIG_DISTANCE_FEE_PER_KM", 2000m },
            { "CONFIG_WEIGHT_FEE_PER_KG", 5000m },
            { "CONFIG_FREE_SHIPPING_THRESHOLD", 300000m },
            { "CONFIG_REMOTE_AREA_SURCHARGE", 20000m }
        };

        var regions = desiredConfigs.Keys.ToArray();
        var existingConfigs = await db.ShippingFees
            .Where(f => regions.Contains(f.Region))
            .ToListAsync(ct);

        var configsByRegion = existingConfigs.ToDictionary(f => f.Region);
        var created = 0;

        foreach (var (region, amount) in desiredConfigs)
        {
            if (configsByRegion.TryGetValue(region, out var existing))
            {
                if (existing.Amount != amount)
                {
                    existing.Amount = amount;
                }
                continue;
            }

            db.ShippingFees.Add(new ShippingFee
            {
                Region = region,
                Amount = amount,
                Currency = "VND"
            });
            created++;
        }

        if (created > 0 || db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(ct);
            logger?.LogInformation("Seeded {Count} shipping configurations.", created);
        }
    }
}
