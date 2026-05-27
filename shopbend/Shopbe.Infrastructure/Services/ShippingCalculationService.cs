using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Shipping.Dtos;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Services;

public class ShippingCalculationService(ShopDbContext db) : IShippingCalculationService
{
    public async Task<ShippingCalculationResponseDto> CalculateAsync(ShippingCalculationRequestDto request, CancellationToken ct = default)
    {
        var city = request.City.Trim();
        var district = request.District.Trim();

        // 1. Resolve Zone
        var mapping = await db.ShippingZoneDistricts
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.City == city && d.District == district, ct);

        var zone = mapping != null 
            ? await db.ShippingZones.AsNoTracking().FirstOrDefaultAsync(z => z.Id == mapping.ZoneId, ct)
            : await db.ShippingZones.AsNoTracking().FirstOrDefaultAsync(z => z.Name == "National Remote", ct);

        decimal baseFee = zone?.Fee ?? 50000m;
        string zoneName = zone?.Name ?? "Unknown";

        // 2. Fetch Configs
        var configs = await db.ShippingFees
            .AsNoTracking()
            .Where(f => f.Region.StartsWith("CONFIG_"))
            .ToDictionaryAsync(f => f.Region, f => f.Amount, ct);

        decimal finalFee = baseFee;

        // 3. Apply Threshold
        if (configs.TryGetValue("CONFIG_FREE_SHIPPING_THRESHOLD", out var threshold) && request.Subtotal >= threshold)
        {
            return new ShippingCalculationResponseDto(0, zoneName, DateTime.UtcNow.AddDays(3));
        }

        // 4. Apply Weight Fee
        if (configs.TryGetValue("CONFIG_WEIGHT_FEE_PER_KG", out var weightRate) && request.TotalWeight > 0)
        {
            finalFee += (decimal)request.TotalWeight * weightRate;
        }

        // 5. Apply Distance Fee
        if (configs.TryGetValue("CONFIG_DISTANCE_FEE_PER_KM", out var distanceRate) && request.DistanceKm > 0)
        {
            finalFee += (decimal)request.DistanceKm * distanceRate;
        }

        // 6. Apply Surcharge for Remote areas
        if (zoneName.Contains("Remote") && configs.TryGetValue("CONFIG_REMOTE_AREA_SURCHARGE", out var surcharge))
        {
            finalFee += surcharge;
        }

        // Round to nearest 1000 for VND
        finalFee = Math.Ceiling(finalFee / 1000m) * 1000m;

        // Estimate Delivery Date
        int daysToAdd = zoneName switch
        {
            var s when s.Contains("Inner") => 2,
            var s when s.Contains("Suburbs") => 3,
            _ => 5
        };

        return new ShippingCalculationResponseDto(finalFee, zoneName, DateTime.UtcNow.AddDays(daysToAdd));
    }
}
