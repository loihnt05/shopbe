using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Entities.Product;
using Shopbe.Domain.Entities.ShoppingCart;
using Shopbe.Domain.Entities.User;
using Shopbe.Infrastructure.Persistence;

// Usage examples:
//   dotnet run --project shopbend/Shopbe.Seeder -- --connection "Host=localhost;Port=5432;Database=shopbe;Username=shopbe;Password=shopbe" \
//     --products 20000 --categories 50 --brands 200 --users 5000 --orders 20000
//
// Notes:
// - This is intended for dev/staging environments.
// - It is safe to re-run with --mode ensure (default): it will top-up counts based on existing rows.

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
    bool Verbose)
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

        return (new SeedOptions(conn, categories, brands, products, variantsMin, variantsMax, users, orders, maxItems, migrate, mode, seed, batch, verbose),
            errors);
    }
}

internal sealed class ShopbeLargeDataSeeder
{
    private sealed record VariantRow(Guid Id, string Sku, string ProductName, decimal Price);

    public async Task WipeAsync(ShopDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // Order matters due to FK constraints.
        // Note: we keep this conservative and only delete core data sets.
        var tables = new[]
        {
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
                // t is a constant from our hard-coded list above (not user input).
                // Avoid string interpolation directly in SQL (EF1002): use parameterized SQL.
                await db.Database.ExecuteSqlRawAsync(
                    "TRUNCATE TABLE {0} RESTART IDENTITY CASCADE;",
                    parameters: [t],
                    cancellationToken: ct);
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

        await EnsureCategoriesAsync(db, logger, options, ct);
        await EnsureBrandsAsync(db, logger, options, ct);
        await EnsureProductsAsync(db, logger, options, ct);
        await EnsureUsersAsync(db, logger, options, ct);
        await EnsureOrdersAsync(db, logger, options, ct);
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
            var name = faker.Commerce.Categories(1)[0];
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
            var name = UniqueName(faker.Company.CompanyName(), usedNames);
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
        var categoryIds = await db.Categories.AsNoTracking().Select(c => c.Id).ToListAsync(ct);
        var brandIds = await db.Brands.AsNoTracking().Select(b => b.Id).ToListAsync(ct);
        if (categoryIds.Count == 0) throw new InvalidOperationException("No categories found. Seed categories first.");
        if (brandIds.Count == 0) throw new InvalidOperationException("No brands found. Seed brands first.");

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
            var name = faker.Commerce.ProductName();
            var slug = UniqueSlug(Slugify(name), usedProductSlugs);
            var basePrice = decimal.Parse(faker.Commerce.Price(5, 500)) * 1000m;
            var categoryId = categoryIds[faker.Random.Int(0, categoryIds.Count - 1)];
            var brandId = brandIds[faker.Random.Int(0, brandIds.Count - 1)];

            var p = new Product
            {
                Name = name,
                Slug = slug,
                Description = faker.Commerce.ProductDescription(),
                BasePrice = basePrice,
                CategoryId = categoryId,
                BrandId = brandId,
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
                ImageUrl = $"https://picsum.photos/seed/{p.Slug}/800/800",
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
                    Price = p.BasePrice + faker.Random.Decimal(-10000m, 50000m),
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
                Role = null,
                Status = null
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
                (v, p) => new VariantRow(v.Id, v.Sku, p.Name, v.Price))
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
                    TotalPrice = lineTotal
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
        var sku = skuBase;
        var i = 1;
        while (!used.Add(sku))
        {
            sku = $"{skuBase}_{i++}";
        }
        return sku;
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







