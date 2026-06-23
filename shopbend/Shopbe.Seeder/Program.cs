using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Entities.Product;
using Shopbe.Domain.Entities.Seller;
using Shopbe.Domain.Entities.ShoppingCart;
using Shopbe.Domain.Entities.User;
using Shopbe.Domain.Enums;
using Shopbe.Infrastructure.Persistence;

// Usage examples:
//   dotnet run --project shopbend/Shopbe.Seeder -- --connection "Host=localhost;Port=5432;Database=shopbe;Username=shopbe;Password=shopbe" \
//     --products 20000 --categories 50 --brands 200 --users 5000 --orders 20000
//
// Notes:
// - This is intended for dev/staging environments.
// - It is safe to re-run with --mode ensure (default): it will top-up counts based on existing rows.
// - By default it creates a large polished no-network catalog. Use --use-dummy true to pull a smaller external catalog.

var (options, parseErrors) = SeedOptions.Parse(args);
if (parseErrors.Count > 0)
{
    Console.Error.WriteLine("Invalid arguments:\n - " + string.Join("\n - ", parseErrors));
    Console.Error.WriteLine();
    Console.Error.WriteLine(SeedOptions.HelpText);
    return 2;
}

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: true);
        cfg.AddEnvironmentVariables(prefix: "SHOPBE_");
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.TimestampFormat = "HH:mm:ss ";
        });
        logging.SetMinimumLevel(options.Verbose ? LogLevel.Information : LogLevel.Warning);
    })
    .ConfigureServices((ctx, services) =>
    {
        var conn = options.ConnectionString
                   ?? ctx.Configuration["ConnectionString"]
                   ?? ctx.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(conn))
        {
            throw new InvalidOperationException(
                "Missing connection string. Provide --connection or set SHOPBE_ConnectionString or ConnectionStrings:DefaultConnection.");
        }

        services.AddDbContext<ShopDbContext>(db => db.UseNpgsql(conn));
        services.AddSingleton(options);
        services.AddSingleton<ShopbeLargeDataSeeder>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Shopbe.Seeder");
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<ShopbeLargeDataSeeder>();

    if (options.Migrate)
    {
        logger.LogWarning("Applying migrations...");
        await db.Database.MigrateAsync();
    }

    if (options.Mode == SeedMode.WipeAndReseed)
    {
        logger.LogWarning("WIPE mode enabled. Deleting data (except migrations history)...");
        await seeder.WipeAsync(db, logger);
    }

    var sw = Stopwatch.StartNew();
    await seeder.SeedAsync(db, logger, options);
    sw.Stop();

    logger.LogWarning("Seeding complete in {Elapsed}.", sw.Elapsed);
}

return 0;

internal enum SeedMode
{
    Ensure = 0,
    WipeAndReseed = 1
}

internal sealed record SeedOptions(
    string? ConnectionString,
    int Categories,
    int Brands,
    int Products,
    int VariantsPerProductMin,
    int VariantsPerProductMax,
    int Users,
    int Orders,
    int MaxItemsPerOrder,
    bool Migrate,
    SeedMode Mode,
    int RandomSeed,
    int BatchSize,
    bool Verbose,
    bool UseDummy)
{
    public static string HelpText =>
        "Shopbe.Seeder - large data generator\n" +
        "Options:\n" +
        "  --connection <conn>         PostgreSQL connection string\n" +
        "  --mode ensure|wipe          ensure (top-up) or wipe (delete + reseed)\n" +
        "  --migrate true|false        apply EF migrations before seeding (default: true)\n" +
        "  --categories <n>            target category count (default: 50)\n" +
        "  --brands <n>                target brand count (default: 200)\n" +
        "  --products <n>              target product count (default: 20000)\n" +
        "  --variants-min <n>          variants per product min (default: 1)\n" +
        "  --variants-max <n>          variants per product max (default: 5)\n" +
        "  --users <n>                 target user count (default: 5000)\n" +
        "  --orders <n>                target order count (default: 20000)\n" +
        "  --max-items <n>             max items per order (default: 5)\n" +
        "  --batch <n>                 EF SaveChanges batch size (default: 2000)\n" +
        "  --seed <n>                  random seed (default: 1337)\n" +
        "  --use-dummy [true|false]    use dummyjson.com for products instead of the polished local generator (default: false)\n" +
        "  --verbose                   info logs\n";

    public static (SeedOptions options, List<string> errors) Parse(string[] args)
    {
        string? conn = null;
        var errors = new List<string>();

        int categories = 50;
        int brands = 200;
        int products = 20000;
        int variantsMin = 1;
        int variantsMax = 5;
        int users = 5000;
        int orders = 20000;
        int maxItems = 5;
        bool migrate = true;
        var mode = SeedMode.Ensure;
        int seed = 1337;
        int batch = 2000;
        bool verbose = false;
        bool useDummy = false;

        for (var i = 0; i < args.Length; i++)
        {
            var a = args[i];
            string Next()
            {
                if (i + 1 >= args.Length) throw new ArgumentException($"Missing value for {a}");
                return args[++i];
            }

            try
            {
                switch (a)
                {
                    case "--connection":
                    case "-c":
                        conn = Next();
                        break;
                    case "--categories":
                        categories = int.Parse(Next());
                        break;
                    case "--brands":
                        brands = int.Parse(Next());
                        break;
                    case "--products":
                        products = int.Parse(Next());
                        break;
                    case "--variants-min":
                        variantsMin = int.Parse(Next());
                        break;
                    case "--variants-max":
                        variantsMax = int.Parse(Next());
                        break;
                    case "--users":
                        users = int.Parse(Next());
                        break;
                    case "--orders":
                        orders = int.Parse(Next());
                        break;
                    case "--max-items":
                        maxItems = int.Parse(Next());
                        break;
                    case "--migrate":
                        migrate = bool.Parse(Next());
                        break;
                    case "--mode":
                        mode = Next().ToLowerInvariant() switch
                        {
                            "ensure" => SeedMode.Ensure,
                            "wipe" => SeedMode.WipeAndReseed,
                            _ => throw new ArgumentException("mode must be ensure|wipe")
                        };
                        break;
                    case "--seed":
                        seed = int.Parse(Next());
                        break;
                    case "--batch":
                        batch = int.Parse(Next());
                        break;
                    case "--use-dummy":
                        if (i + 1 < args.Length && bool.TryParse(args[i + 1], out var val))
                        {
                            useDummy = val;
                            i++;
                        }
                        else
                        {
                            useDummy = true;
                        }
                        break;
                    case "--verbose":
                        verbose = true;
                        break;
                    case "--help":
                    case "-h":
                        Console.WriteLine(HelpText);
                        Environment.Exit(0);
                        break;
                    default:
                        // ignore unknown args that Host may pass
                        break;
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
        }

        if (variantsMin < 1) errors.Add("--variants-min must be >= 1");
        if (variantsMax < variantsMin) errors.Add("--variants-max must be >= --variants-min");
        if (batch < 1) errors.Add("--batch must be >= 1");

        return (new SeedOptions(conn, categories, brands, products, variantsMin, variantsMax, users, orders, maxItems, migrate, mode, seed, batch, verbose, useDummy),
            errors);
    }
}

internal sealed class ShopbeLargeDataSeeder
{
    private sealed record VariantRow(Guid Id, string Sku, string ProductName, decimal Price, Guid SellerId);
    private sealed record CatalogCategory(string Name, string Slug, string[] SearchTerms);
    private sealed record ProductTemplate(string CategorySlug, string Brand, string[] ProductTypes, string[] Adjectives, string[] Materials);

    private static readonly CatalogCategory[] BeautifulCategories =
    [
        new("Smart Tech", "smart-tech", ["wireless earbuds", "smart watch", "laptop desk setup", "smart home device"]),
        new("Home Studio", "home-studio", ["minimal home decor", "ceramic vase", "desk lamp", "linen bedding"]),
        new("Modern Wardrobe", "modern-wardrobe", ["fashion apparel", "leather bag", "sneakers", "cotton shirt"]),
        new("Beauty Lab", "beauty-lab", ["skincare bottle", "cosmetics", "perfume", "beauty tools"]),
        new("Active Life", "active-life", ["fitness gear", "running shoes", "yoga mat", "sports bottle"]),
        new("Kitchen Craft", "kitchen-craft", ["kitchenware", "coffee maker", "ceramic plate", "cookware"]),
        new("Travel Ready", "travel-ready", ["travel backpack", "luggage", "camera bag", "weekend bag"]),
        new("Kids & Baby", "kids-baby", ["wooden toy", "baby clothing", "kids room", "plush toy"]),
        new("Pet Comfort", "pet-comfort", ["pet bed", "pet bowl", "dog toy", "cat furniture"]),
        new("Outdoor Living", "outdoor-living", ["camping gear", "outdoor chair", "garden light", "picnic set"])
    ];

    private static readonly string[] BeautifulBrands =
    [
        "Aurel Studio", "Luna & Co", "Northline", "Vera Haus", "Orchid Works", "Cobalt Lane",
        "Mira Mode", "Atlas Goods", "Velvet Ridge", "Sora Living", "Kinfolk Supply", "Nimbus Lab",
        "Evermade", "Solace Home", "Hearth & Harbor", "Brightside", "Terra & Thread", "Nova Kit",
        "Bloom Atelier", "Peak Theory", "Kindred Baby", "Paw & Porter", "Urban Nomad", "Olive Works"
    ];

    private static readonly ProductTemplate[] ProductTemplates =
    [
        new("smart-tech", "Nimbus Lab", ["Wireless Earbuds", "Smart Watch", "Portable Speaker", "Charging Dock", "Mechanical Keyboard", "USB-C Hub"], ["Aurora", "Pulse", "Studio", "Pro", "Slim"], ["aluminum", "matte polymer", "braided nylon"]),
        new("home-studio", "Sora Living", ["Ceramic Vase", "Desk Lamp", "Linen Sheet Set", "Storage Basket", "Wall Shelf", "Scented Candle"], ["Calm", "Nordic", "Cloud", "Dawn", "Soft"], ["ceramic", "oak", "linen", "brass"]),
        new("modern-wardrobe", "Mira Mode", ["Cotton Shirt", "Leather Tote", "Everyday Sneakers", "Tailored Jacket", "Ribbed Knit Top", "Crossbody Bag"], ["Essential", "Atelier", "City", "Weekend", "Signature"], ["cotton", "leather", "suede", "canvas"]),
        new("beauty-lab", "Bloom Atelier", ["Hydrating Serum", "Silk Hair Mask", "Mineral Sunscreen", "Body Oil", "Perfume Mist", "Makeup Brush Set"], ["Radiant", "Dew", "Velvet", "Rose", "Glow"], ["glass", "botanical oils", "mineral blend"]),
        new("active-life", "Peak Theory", ["Training Shoes", "Yoga Mat", "Insulated Bottle", "Run Shorts", "Resistance Band Kit", "Gym Duffel"], ["Motion", "Core", "Sprint", "Flex", "Trail"], ["recycled polyester", "rubber", "stainless steel"]),
        new("kitchen-craft", "Hearth & Harbor", ["Pour-Over Kettle", "Stoneware Dinner Set", "Chef Knife", "Coffee Grinder", "Cast Iron Pan", "Glass Storage Set"], ["Heritage", "Morning", "Craft", "Classic", "Pure"], ["stoneware", "stainless steel", "walnut", "borosilicate glass"]),
        new("travel-ready", "Urban Nomad", ["Carry-On Luggage", "Travel Backpack", "Packing Cube Set", "Camera Sling", "Passport Wallet", "Weekender Bag"], ["Voyage", "Metro", "Summit", "Roam", "Transit"], ["ripstop nylon", "vegan leather", "polycarbonate"]),
        new("kids-baby", "Kindred Baby", ["Organic Onesie", "Wooden Stacker", "Nursery Blanket", "Bath Towel Set", "Story Book Set", "Soft Play Mat"], ["Tiny", "Cozy", "Sunny", "Gentle", "Little"], ["organic cotton", "beech wood", "muslin"]),
        new("pet-comfort", "Paw & Porter", ["Pet Bed", "Ceramic Food Bowl", "Walking Harness", "Treat Pouch", "Cat Scratcher", "Plush Toy Set"], ["Happy", "Comfy", "Daily", "Snuggle", "Trail"], ["canvas", "ceramic", "fleece", "vegan leather"]),
        new("outdoor-living", "Terra & Thread", ["Camping Chair", "Solar Garden Light", "Picnic Blanket", "Cooler Bag", "Lantern", "Outdoor Planter"], ["Field", "Camp", "Garden", "Sunset", "Trail"], ["powder-coated steel", "waxed canvas", "rattan", "recycled plastic"])
    ];

    public async Task WipeAsync(ShopDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Order matters due to FK constraints.
        // Note: we keep this conservative and only delete core data sets.
        var tables = new[]
        {
            "\"AuditLogs\"",
            "\"EmailMessages\"",
            "\"NotificationLogs\"",
            "\"Notifications\"",
            "\"ChatMessages\"",
            "\"Conversations\"",
            "\"UserBehaviors\"",
            "\"WishlistItems\"",
            "\"ReviewImages\"",
            "\"Reviews\"",
            "\"ShippingZoneDistricts\"",
            "\"ShippingZones\"",
            "\"ShippingFees\"",
            "\"Shipments\"",
            "\"IdempotencyKeys\"",
            "\"Refunds\"",
            "\"PaymentLogs\"",
            "\"PaymentTransactions\"",
            "\"CouponUsages\"",
            "\"Coupons\"",
            "\"OrderStatusHistory\"",
            "\"InventoryTransactions\"",
            "\"OrderItems\"",
            "\"Orders\"",
            "\"CartItems\"",
            "\"ShoppingCarts\"",
            "\"UserAddresses\"",
            "\"Users\"",
            "\"ProductVariantAttributes\"",
            "\"AttributeValues\"",
            "\"ProductAttributes\"",
            "\"ProductVariants\"",
            "\"ProductImages\"",
            "\"Products\"",
            "\"Brands\"",
            "\"Categories\""
        };

        foreach (var t in tables)
        {
            try
            {
                // Note: PostgreSQL does NOT allow parameterizing table names (e.g. TRUNCATE TABLE @p0).
                // Since 'tables' is a hard-coded list above, we can safely use string interpolation.
#pragma warning disable EF1002
                await db.Database.ExecuteSqlRawAsync(
                    $"TRUNCATE TABLE {t} RESTART IDENTITY CASCADE;",
                    cancellationToken: ct);
#pragma warning restore EF1002
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to truncate {Table} (may not exist in current migration set).", t);
            }
        }
    }

    public async Task SeedAsync(ShopDbContext db, ILogger logger, SeedOptions options, CancellationToken ct = default)
    {
        Bogus.Randomizer.Seed = new Random(options.RandomSeed);

        await SeedShippingLocationsAsync(db, logger, ct);
        await SeedAttributesAsync(db, logger, ct);
        await EnsureCoreUsersAndSellersAsync(db, logger, ct);

        if (options.UseDummy)
        {
            await DummyJsonSeeder.SeedAsync(db, logger, 54, ct);
            await EscuelaSeeder.SeedAsync(db, logger, ct);
        }
        else
        {
            await EnsureCategoriesAsync(db, logger, options, ct);
            await EnsureBrandsAsync(db, logger, options, ct);
            await EnsureProductsAsync(db, logger, options, ct);
        }

        await EnsureUsersAsync(db, logger, options, ct);
        await SeedCouponsAsync(db, logger, ct);
        
        await AssignRandomAttributesToVariantsAsync(db, logger, ct);
        
        await EnsureOrdersAsync(db, logger, options, ct);
    }

    private static async Task SeedAttributesAsync(ShopDbContext db, ILogger logger, CancellationToken ct)
    {
        var attributes = new Dictionary<string, string[]>
        {
            { "Color", new[] { "Red", "Blue", "Green", "Black", "White", "Silver", "Gold", "Pink", "Purple", "Yellow", "Orange", "Grey", "Brown" } },
            { "Size", new[] { "S", "M", "L", "XL", "XXL", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45" } },
            { "Material", new[] { "Cotton", "Silk", "Leather", "Denim", "Polyester", "Nylon", "Wool", "Plastic", "Metal", "Wood", "Glass" } },
            { "Storage", new[] { "64GB", "128GB", "256GB", "512GB", "1TB" } },
            { "RAM", new[] { "4GB", "8GB", "16GB", "32GB" } }
        };

        var createdAttr = 0;
        var createdVals = 0;

        foreach (var attrPair in attributes)
        {
            var attrName = attrPair.Key;
            var values = attrPair.Value;

            var existingAttr = await db.ProductAttributes
                .Include(a => a.AttributeValues)
                .FirstOrDefaultAsync(a => a.Name == attrName, ct);

            if (existingAttr == null)
            {
                existingAttr = new ProductAttribute { Name = attrName };
                db.ProductAttributes.Add(existingAttr);
                createdAttr++;
            }

            foreach (var val in values)
            {
                if (existingAttr.AttributeValues.All(v => v.Value != val))
                {
                    db.AttributeValues.Add(new AttributeValue
                    {
                        AttributeId = existingAttr.Id,
                        Value = val
                    });
                    createdVals++;
                }
            }
        }

        if (createdAttr > 0 || createdVals > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogWarning("Seeded {AttrCount} attributes and {ValCount} attribute values.", createdAttr, createdVals);
        }
        else
        {
            logger.LogWarning("Attributes: ok ({Count}).", await db.ProductAttributes.CountAsync(ct));
        }
    }

    private static async Task AssignRandomAttributesToVariantsAsync(ShopDbContext db, ILogger logger, CancellationToken ct)
    {
        // Only assign if we have variants and no assignments yet
        if (await db.ProductVariantAttributes.AnyAsync(ct))
        {
            logger.LogWarning("Variant Attributes: ok ({Count}).", await db.ProductVariantAttributes.CountAsync(ct));
            return;
        }

        var variants = await db.ProductVariants.ToListAsync(ct);
        if (variants.Count == 0) return;

        var colors = await db.AttributeValues
            .Where(v => v.Attribute!.Name == "Color")
            .ToListAsync(ct);
        var sizes = await db.AttributeValues
            .Where(v => v.Attribute!.Name == "Size")
            .ToListAsync(ct);
        var materials = await db.AttributeValues
            .Where(v => v.Attribute!.Name == "Material")
            .ToListAsync(ct);

        var random = new Random();
        var assignments = 0;

        foreach (var variant in variants)
        {
            var possibleValues = new List<AttributeValue>();
            
            var roll = random.Next(100);
            if (roll < 40) possibleValues.Add(colors[random.Next(colors.Count)]);
            
            roll = random.Next(100);
            if (roll < 30) possibleValues.Add(sizes[random.Next(sizes.Count)]);

            roll = random.Next(100);
            if (roll < 20) possibleValues.Add(materials[random.Next(materials.Count)]);

            foreach (var val in possibleValues)
            {
                db.ProductVariantAttributes.Add(new ProductVariantAttribute
                {
                    VariantId = variant.Id,
                    AttributeValueId = val.Id
                });
                assignments++;
            }
        }

        if (assignments > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogWarning("Randomly assigned {Count} attributes to product variants.", assignments);
        }
    }

    private static async Task SeedCouponsAsync(ShopDbContext db, ILogger logger, CancellationToken ct)
    {
        var desiredCoupons = new List<Coupon>
        {
            new()
            {
                Code = "HELLO2026",
                Description = "10% off for new year",
                DiscountType = Shopbe.Domain.Enums.DiscountType.Percentage,
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
                DiscountType = Shopbe.Domain.Enums.DiscountType.FreeShipping,
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
                DiscountType = Shopbe.Domain.Enums.DiscountType.FixedAmount,
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
                DiscountType = Shopbe.Domain.Enums.DiscountType.Percentage,
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
                DiscountType = Shopbe.Domain.Enums.DiscountType.Percentage,
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
                DiscountType = Shopbe.Domain.Enums.DiscountType.FixedAmount,
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
                DiscountType = Shopbe.Domain.Enums.DiscountType.Percentage,
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
                DiscountType = Shopbe.Domain.Enums.DiscountType.Percentage,
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
            logger.LogWarning("Seeded {Count} sample coupons.", created);
        }
        else
        {
            logger.LogWarning("Coupons: ok ({Count}).", await db.Coupons.CountAsync(ct));
        }
    }

    private static async Task SeedShippingLocationsAsync(ShopDbContext db, ILogger logger, CancellationToken ct)
    {
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
                if (existing.Fee != fee)
                {
                    existing.Fee = fee;
                }
                continue;
            }

            var zone = new Shopbe.Domain.Entities.Shipping.ShippingZone { Name = name, Fee = fee };
            db.ShippingZones.Add(zone);
            zonesByName[name] = zone;
            createdZones++;
        }

        if (createdZones > 0)
        {
            await db.SaveChangesAsync(ct);
        }

        // Create district mappings
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
                continue;
            }

            var exists = await db.ShippingZoneDistricts.AsNoTracking().AnyAsync(d =>
                d.ZoneId == zone.Id && d.City == city && d.District == district, ct);

            if (exists)
            {
                continue;
            }

            db.ShippingZoneDistricts.Add(new Shopbe.Domain.Entities.Shipping.ShippingZoneDistrict
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
            logger.LogWarning("Seeded shipping locations: +{Zones} zones, +{Mappings} mappings.", createdZones, createdMappings);
        }
    }

    private static async Task EnsureCategoriesAsync(ShopDbContext db, ILogger logger, SeedOptions options, CancellationToken ct)
    {
        var existing = await db.Categories.CountAsync(ct);
        var toCreate = Math.Max(0, options.Categories - existing);
        if (toCreate == 0)
        {
            logger.LogWarning("Categories: ok ({Count}).", existing);
            return;
        }

        var faker = new Bogus.Faker("en");
        var usedSlugs = new HashSet<string>(
            await db.Categories.AsNoTracking().Select(c => c.Slug).ToListAsync(ct),
            StringComparer.OrdinalIgnoreCase);

        var batch = new List<Category>(Math.Min(toCreate, options.BatchSize));
        for (var i = 0; i < toCreate; i++)
        {
            var curated = i < BeautifulCategories.Length ? BeautifulCategories[i] : null;
            var name = curated?.Name ?? faker.Commerce.Categories(1)[0];
            var slugBase = Slugify(name);
            var slug = UniqueSlug(slugBase, usedSlugs);

            batch.Add(new Category
            {
                Name = name,
                Slug = slug,
                SortOrder = existing + i + 1,
                IsActive = true
            });

            if (batch.Count >= options.BatchSize)
            {
                db.Categories.AddRange(batch);
                await db.SaveChangesAsync(ct);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            db.Categories.AddRange(batch);
            await db.SaveChangesAsync(ct);
        }

        logger.LogWarning("Categories: created {Created} (total {Total}).", toCreate, await db.Categories.CountAsync(ct));
    }

    private static async Task EnsureBrandsAsync(ShopDbContext db, ILogger logger, SeedOptions options, CancellationToken ct)
    {
        var existing = await db.Brands.CountAsync(ct);
        var toCreate = Math.Max(0, options.Brands - existing);
        if (toCreate == 0)
        {
            logger.LogWarning("Brands: ok ({Count}).", existing);
            return;
        }

        var faker = new Bogus.Faker("en");
        var usedSlugs = new HashSet<string>(
            await db.Brands.AsNoTracking().Select(b => b.Slug).ToListAsync(ct),
            StringComparer.OrdinalIgnoreCase);

        var usedNames = new HashSet<string>(
            await db.Brands.AsNoTracking().Select(b => b.Name).ToListAsync(ct),
            StringComparer.OrdinalIgnoreCase);

        var batch = new List<Brand>(Math.Min(toCreate, options.BatchSize));
        for (var i = 0; i < toCreate; i++)
        {
            var baseName = i < BeautifulBrands.Length ? BeautifulBrands[i] : faker.Company.CompanyName();
            var name = UniqueName(baseName, usedNames);
            var slug = UniqueSlug(Slugify(name), usedSlugs);
            batch.Add(new Brand
            {
                Name = name,
                Slug = slug,
                LogoUrl = null,
                IsActive = true
            });

            if (batch.Count >= options.BatchSize)
            {
                db.Brands.AddRange(batch);
                await db.SaveChangesAsync(ct);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            db.Brands.AddRange(batch);
            await db.SaveChangesAsync(ct);
        }

        logger.LogWarning("Brands: created {Created} (total {Total}).", toCreate, await db.Brands.CountAsync(ct));
    }

    private static async Task EnsureProductsAsync(ShopDbContext db, ILogger logger, SeedOptions options, CancellationToken ct)
    {
        var existing = await db.Products.CountAsync(ct);
        var toCreate = Math.Max(0, options.Products - existing);
        if (toCreate == 0)
        {
            logger.LogWarning("Products: ok ({Count}).", existing);
            return;
        }

        var faker = new Bogus.Faker("en");
        var categories = await db.Categories.AsNoTracking().Select(c => new { c.Id, c.Name, c.Slug }).ToListAsync(ct);
        var brands = await db.Brands.AsNoTracking().Select(b => new { b.Id, b.Name, b.Slug }).ToListAsync(ct);
        var sellerIds = await db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.Seller || u.Role == UserRole.Admin)
            .Select(u => u.Id)
            .ToListAsync(ct);
        if (categories.Count == 0) throw new InvalidOperationException("No categories found. Seed categories first.");
        if (brands.Count == 0) throw new InvalidOperationException("No brands found. Seed brands first.");
        if (sellerIds.Count == 0) throw new InvalidOperationException("No sellers found. Seed core users first.");

        var usedProductSlugs = new HashSet<string>(
            await db.Products.AsNoTracking().Select(p => p.Slug).ToListAsync(ct),
            StringComparer.OrdinalIgnoreCase);

        var usedSkus = new HashSet<string>(
            await db.ProductVariants.AsNoTracking().Select(v => v.Sku).ToListAsync(ct),
            StringComparer.OrdinalIgnoreCase);

        // Insert in batches: Products first (get Ids), then variants & images.
        var productBatch = new List<Product>(Math.Min(toCreate, options.BatchSize));
        var variantsBatch = new List<ProductVariant>(options.BatchSize * Math.Max(1, options.VariantsPerProductMax));
        var imagesBatch = new List<ProductImage>(options.BatchSize);

        for (var i = 0; i < toCreate; i++)
        {
            var template = ProductTemplates[faker.Random.Int(0, ProductTemplates.Length - 1)];
            var productType = faker.PickRandom(template.ProductTypes);
            var adjective = faker.PickRandom(template.Adjectives);
            var material = faker.PickRandom(template.Materials);
            var color = faker.PickRandom("Black", "White", "Sage", "Clay", "Navy", "Ivory", "Walnut", "Blush", "Graphite");
            var collection = faker.PickRandom("Studio", "Daily", "Market", "Reserve", "Classic", "Edition", "Select", "Prime");
            var name = $"{adjective} {productType} - {color} {collection}";
            var slug = UniqueSlug(Slugify(name), usedProductSlugs);
            var basePrice = decimal.Round(faker.Random.Decimal(90_000m, 4_500_000m) / 1000m, 0) * 1000m;
            var category = categories.FirstOrDefault(c => c.Slug == template.CategorySlug)
                ?? categories[faker.Random.Int(0, categories.Count - 1)];
            var brand = brands.FirstOrDefault(b => string.Equals(b.Name, template.Brand, StringComparison.OrdinalIgnoreCase))
                ?? brands[faker.Random.Int(0, brands.Count - 1)];
            var sellerId = sellerIds[faker.Random.Int(0, sellerIds.Count - 1)];

            var p = new Product
            {
                Name = name,
                Slug = slug,
                Description = $"A polished {material} {productType.ToLowerInvariant()} designed for everyday use, styled for the ShopBee demo catalog.",
                BasePrice = basePrice,
                DiscountPrice = faker.Random.Int(0, 100) < 30 ? decimal.Round(basePrice * faker.Random.Decimal(0.82m, 0.94m) / 1000m, 0) * 1000m : null,
                SoldCount = faker.Random.Int(0, 1800),
                CategoryId = category.Id,
                BrandId = brand.Id,
                SellerId = sellerId,
                ApprovalStatus = ApprovalStatus.Approved,
                IsActive = true
            };
            productBatch.Add(p);

            if (productBatch.Count >= options.BatchSize)
            {
                await FlushProductsAsync(db, faker, productBatch, variantsBatch, imagesBatch, usedSkus, options, ct);
            }
        }

        if (productBatch.Count > 0)
        {
            await FlushProductsAsync(db, faker, productBatch, variantsBatch, imagesBatch, usedSkus, options, ct);
        }

        logger.LogWarning("Products: created {Created} (total {Total}), variants total {Variants}.",
            toCreate,
            await db.Products.CountAsync(ct),
            await db.ProductVariants.CountAsync(ct));
    }

    private static async Task FlushProductsAsync(
        ShopDbContext db,
        Bogus.Faker faker,
        List<Product> productBatch,
        List<ProductVariant> variantsBatch,
        List<ProductImage> imagesBatch,
        HashSet<string> usedSkus,
        SeedOptions options,
        CancellationToken ct)
    {
        db.Products.AddRange(productBatch);
        await db.SaveChangesAsync(ct);

        // Now that Product IDs exist, build dependent rows.
        foreach (var p in productBatch)
        {
            imagesBatch.Add(new ProductImage
            {
                ProductId = p.Id,
                ImageUrl = BuildProductImageUrl(p, faker),
                AltText = p.Name,
                IsPrimary = true,
                SortOrder = 0
            });

            var variantCount = faker.Random.Int(options.VariantsPerProductMin, options.VariantsPerProductMax);
            for (var v = 0; v < variantCount; v++)
            {
                var skuBase = $"{p.Slug.ToUpperInvariant().Replace('-', '_')}_{v + 1}";
                var sku = UniqueSku(skuBase, usedSkus);
                variantsBatch.Add(new ProductVariant
                {
                    ProductId = p.Id,
                    Sku = sku,
                    Price = Math.Max(1000m, p.BasePrice + faker.Random.Decimal(-20000m, 120000m)),
                    StockQuantity = faker.Random.Int(0, 500),
                    IsActive = true
                });
            }
        }

        db.ProductImages.AddRange(imagesBatch);
        db.ProductVariants.AddRange(variantsBatch);
        await db.SaveChangesAsync(ct);

        productBatch.Clear();
        imagesBatch.Clear();
        variantsBatch.Clear();
    }

    private static async Task EnsureUsersAsync(ShopDbContext db, ILogger logger, SeedOptions options, CancellationToken ct)
    {
        var existing = await db.Users.CountAsync(ct);
        var toCreate = Math.Max(0, options.Users - existing);
        if (toCreate == 0)
        {
            logger.LogWarning("Users: ok ({Count}).", existing);
            return;
        }

        var faker = new Bogus.Faker("en");
        var usedEmails = new HashSet<string>(
            await db.Users.AsNoTracking().Select(u => u.Email).ToListAsync(ct),
            StringComparer.OrdinalIgnoreCase);

        var batchUsers = new List<User>(Math.Min(toCreate, options.BatchSize));
        var batchAddresses = new List<UserAddress>(options.BatchSize * 2);
        var batchCarts = new List<ShoppingCart>(options.BatchSize);

        for (var i = 0; i < toCreate; i++)
        {
            var first = faker.Name.FirstName();
            var last = faker.Name.LastName();
            var emailBase = faker.Internet.Email(first, last);
            var email = UniqueEmail(emailBase, usedEmails);

            // The project syncs Keycloak users via middleware; for load testing we can seed app users directly.
            // KeycloakId is the sub claim.
            var keycloakId = Guid.NewGuid().ToString();

            var user = new User
            {
                KeycloakId = keycloakId,
                Email = email,
                FullName = $"{first} {last}",
                PhoneNumber = NormalizePhone(faker.Phone.PhoneNumber()),
                Role = UserRole.Customer,
                Status = UserStatus.Active,
                Country = "Vietnam",
                Language = "vi-VN"
            };
            batchUsers.Add(user);

            if (batchUsers.Count >= options.BatchSize)
            {
                await FlushUsersAsync(db, faker, batchUsers, batchAddresses, batchCarts, ct);
            }
        }

        if (batchUsers.Count > 0)
        {
            await FlushUsersAsync(db, faker, batchUsers, batchAddresses, batchCarts, ct);
        }

        logger.LogWarning("Users: created {Created} (total {Total}).", toCreate, await db.Users.CountAsync(ct));
    }

    private static async Task FlushUsersAsync(
        ShopDbContext db,
        Bogus.Faker faker,
        List<User> batchUsers,
        List<UserAddress> batchAddresses,
        List<ShoppingCart> batchCarts,
        CancellationToken ct)
    {
        db.Users.AddRange(batchUsers);
        await db.SaveChangesAsync(ct);

        foreach (var u in batchUsers)
        {
            // 1-2 addresses
            var addrCount = faker.Random.Int(1, 2);
            for (var a = 0; a < addrCount; a++)
            {
                batchAddresses.Add(new UserAddress
                {
                    UserId = u.Id,
                    ReceiverName = u.FullName,
                    Phone = NormalizePhone(u.PhoneNumber ?? faker.Phone.PhoneNumber()),
                    AddressLine = faker.Address.StreetAddress(),
                    City = faker.Address.City(),
                    District = faker.Address.County(),
                    Ward = faker.Address.SecondaryAddress(),
                    IsDefault = a == 0
                });
            }

            batchCarts.Add(new ShoppingCart
            {
                UserId = u.Id
            });
        }

        db.UserAddresses.AddRange(batchAddresses);
        db.ShoppingCarts.AddRange(batchCarts);
        await db.SaveChangesAsync(ct);

        batchUsers.Clear();
        batchAddresses.Clear();
        batchCarts.Clear();
    }

    private static async Task EnsureOrdersAsync(ShopDbContext db, ILogger logger, SeedOptions options, CancellationToken ct)
    {
        var existing = await db.Orders.CountAsync(ct);
        var toCreate = Math.Max(0, options.Orders - existing);
        if (toCreate == 0)
        {
            logger.LogWarning("Orders: ok ({Count}).", existing);
            return;
        }

        var faker = new Bogus.Faker("en");
        var users = await db.Users.AsNoTracking()
            .Select(u => new { u.Id, u.FullName, u.PhoneNumber })
            .ToListAsync(ct);

        var variantData = await db.ProductVariants.AsNoTracking()
            .Join(db.Products.AsNoTracking(), v => v.ProductId, p => p.Id,
                (v, p) => new VariantRow(v.Id, v.Sku, p.Name, v.Price, p.SellerId))
            .ToListAsync(ct);
        if (users.Count == 0) throw new InvalidOperationException("No users found. Seed users first.");
        if (variantData.Count == 0) throw new InvalidOperationException("No product variants found. Seed products first.");

        var orderBatch = new List<Order>(Math.Min(toCreate, options.BatchSize));
        var itemBatch = new List<OrderItem>(options.BatchSize * options.MaxItemsPerOrder);

        for (var i = 0; i < toCreate; i++)
        {
            var u = users[faker.Random.Int(0, users.Count - 1)];

            var order = new Order
            {
                UserId = u.Id,
                ShippingReceiverName = u.FullName,
                ShippingPhone = NormalizePhone(u.PhoneNumber ?? faker.Phone.PhoneNumber()),
                ShippingAddressLine = faker.Address.StreetAddress(),
                ShippingCity = faker.Address.City(),
                ShippingDistrict = faker.Address.County(),
                ShippingWard = faker.Address.SecondaryAddress(),
                SubtotalAmount = 0m,
                DiscountAmount = 0m,
                ShippingFee = 0m,
                TotalAmount = 0m,
                Currency = "VND",
                Status = faker.PickRandom<Shopbe.Domain.Enums.OrderStatus>()
            };
            orderBatch.Add(order);

            if (orderBatch.Count >= options.BatchSize)
            {
                await FlushOrdersAsync(db, faker, orderBatch, itemBatch, variantData, options, ct);
            }
        }

        if (orderBatch.Count > 0)
        {
            await FlushOrdersAsync(db, faker, orderBatch, itemBatch, variantData, options, ct);
        }

        logger.LogWarning("Orders: created {Created} (total {Total}), order items total {Items}.",
            toCreate,
            await db.Orders.CountAsync(ct),
            await db.OrderItems.CountAsync(ct));
    }

    private static async Task FlushOrdersAsync(
        ShopDbContext db,
        Bogus.Faker faker,
        List<Order> orderBatch,
        List<OrderItem> itemBatch,
        List<VariantRow> variantData,
        SeedOptions options,
        CancellationToken ct)
    {
        db.Orders.AddRange(orderBatch);
        await db.SaveChangesAsync(ct);

        foreach (var o in orderBatch)
        {
            var items = faker.Random.Int(1, options.MaxItemsPerOrder);
            decimal subtotal = 0;
            for (var j = 0; j < items; j++)
            {
                var v = variantData[faker.Random.Int(0, variantData.Count - 1)];
                var qty = faker.Random.Int(1, 3);
                var unitPrice = v.Price;
                var lineTotal = unitPrice * qty;
                subtotal += lineTotal;
                itemBatch.Add(new OrderItem
                {
                    OrderId = o.Id,
                    ProductVariantId = v.Id,
                    SkuSnapshot = v.Sku,
                    ProductNameSnapshot = v.ProductName,
                    Quantity = qty,
                    UnitPrice = unitPrice,
                    TotalPrice = lineTotal,
                    SellerId = v.SellerId
                });
            }

            o.SubtotalAmount = subtotal;
            o.DiscountAmount = 0m;
            o.ShippingFee = 0m;
            o.TotalAmount = subtotal;
        }

        db.OrderItems.AddRange(itemBatch);
        await db.SaveChangesAsync(ct);

        orderBatch.Clear();
        itemBatch.Clear();
    }

    private static async Task EnsureCoreUsersAndSellersAsync(ShopDbContext db, ILogger logger, CancellationToken ct)
    {
        var seeds = new[]
        {
            new { Email = "admin@shopbee.vn", Name = "System Admin", Role = UserRole.Admin, Shop = (string?)null, City = "Ho Chi Minh City" },
            new { Email = "seller1@shopbee.vn", Name = "Linh Nguyen", Role = UserRole.Seller, Shop = (string?)"Luna Tech", City = "Ho Chi Minh City" },
            new { Email = "seller2@shopbee.vn", Name = "Minh Tran", Role = UserRole.Seller, Shop = (string?)"Northwind Home", City = "Ha Noi" },
            new { Email = "seller3@shopbee.vn", Name = "An Pham", Role = UserRole.Seller, Shop = (string?)"Bloom Atelier", City = "Da Nang" },
            new { Email = "seller4@shopbee.vn", Name = "Vy Le", Role = UserRole.Seller, Shop = (string?)"Urban Nomad", City = "Ho Chi Minh City" }
        };

        var created = 0;
        foreach (var seed in seeds)
        {
            var user = await db.Users
                .Include(u => u.SellerProfile)
                .Include(u => u.ShoppingCart)
                .FirstOrDefaultAsync(u => u.Email == seed.Email, ct);

            if (user is null)
            {
                user = new User
                {
                    KeycloakId = $"seed-{seed.Email}",
                    Email = seed.Email,
                    FullName = seed.Name,
                    Role = seed.Role,
                    Status = UserStatus.Active,
                    PhoneNumber = "0900000000",
                    Country = "Vietnam",
                    Language = "vi-VN"
                };
                db.Users.Add(user);
                created++;
                await db.SaveChangesAsync(ct);
            }
            else
            {
                user.FullName = seed.Name;
                user.Role = seed.Role;
                user.Status = UserStatus.Active;
                user.Country ??= "Vietnam";
                user.Language ??= "vi-VN";
            }

            user.ShoppingCart ??= new ShoppingCart { UserId = user.Id };

            if (seed.Role == UserRole.Seller)
            {
                user.SellerProfile ??= new SellerProfile { UserId = user.Id };
                user.SellerProfile.ShopName = seed.Shop ?? seed.Name;
                user.SellerProfile.ShopDescription = "Curated ShopBee demo seller with a polished storefront catalog.";
                user.SellerProfile.ContactEmail = seed.Email;
                user.SellerProfile.ContactPhone = user.PhoneNumber;
                user.SellerProfile.Address = "Seeded marketplace address";
                user.SellerProfile.City = seed.City;
                user.SellerProfile.Status = SellerStatus.Active;
                user.SellerProfile.CommissionRate = 0.05m;
                user.SellerProfile.Rating = 4.6m;
            }

            await db.SaveChangesAsync(ct);
        }

        logger.LogWarning("Core users/sellers: ok (+{Created}, total sellers {Sellers}).",
            created,
            await db.Users.CountAsync(u => u.Role == UserRole.Seller, ct));
    }

    private static string Slugify(string value)
    {
        var s = value.Trim().ToLowerInvariant();
        var chars = s.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal)) slug = slug.Replace("--", "-", StringComparison.Ordinal);
        return slug.Trim('-');
    }

    private static string UniqueSlug(string slugBase, HashSet<string> used)
    {
        var slug = string.IsNullOrWhiteSpace(slugBase) ? "item" : slugBase;
        var i = 1;
        while (!used.Add(slug))
        {
            slug = $"{slugBase}-{i++}";
        }
        return slug;
    }

    private static string UniqueSku(string skuBase, HashSet<string> used)
    {
        var sku = LimitLength(skuBase, 100);
        var i = 1;
        while (!used.Add(sku))
        {
            var suffix = $"_{i++}";
            sku = $"{LimitLength(skuBase, 100 - suffix.Length)}{suffix}";
        }
        return sku;
    }

    private static string BuildProductImageUrl(Product product, Bogus.Faker faker)
    {
        var fallbackTerms = BeautifulCategories.SelectMany(c => c.SearchTerms).ToArray();
        var term = faker.PickRandom(fallbackTerms);
        var query = Uri.EscapeDataString($"{term} product");
        return $"https://source.unsplash.com/800x800/?{query}&sig={StableSignature(product.Slug)}";
    }

    private static int StableSignature(string value)
    {
        unchecked
        {
            var hash = 17;
            foreach (var ch in value)
            {
                hash = hash * 31 + ch;
            }

            return Math.Abs(hash);
        }
    }

    private static string LimitLength(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string UniqueEmail(string emailBase, HashSet<string> used)
    {
        var email = emailBase;
        var i = 1;
        while (!used.Add(email))
        {
            var at = emailBase.IndexOf('@');
            email = at > 0
                ? emailBase.Insert(at, $"+{i++}")
                : $"{emailBase}+{i++}@example.com";
        }
        return email;
    }

    private static string UniqueName(string nameBase, HashSet<string> used)
    {
        var name = string.IsNullOrWhiteSpace(nameBase) ? "Brand" : nameBase.Trim();
        var i = 1;
        while (!used.Add(name))
        {
            name = $"{nameBase} {i++}";
        }
        return name;
    }

    private static string NormalizePhone(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "0000000000";
        }

        // Keep digits only.
        var digits = new string(input.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            digits = "0000000000";
        }

        // Ensure it fits into varchar(20) (UserAddressConfiguration).
        return digits.Length <= 20 ? digits : digits[..20];
    }
}



