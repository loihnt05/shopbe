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
        await SeedAttributesAsync(db, logger, ct);

        logger?.LogInformation("Seeding sample catalog data...");

        // Seed from multiple sources. Seeders are idempotent by slug/sku.
        await DummyJsonSeeder.SeedAsync(db, logger, ct: ct);
        await EscuelaSeeder.SeedAsync(db, logger, ct: ct);
        await SeedTestProductsAsync(db, logger, ct);
        await SeedGamingLaptopAsync(db, logger, ct);

        await db.SaveChangesAsync(ct);
        
        // After products/variants are seeded, randomly assign some attributes to them
        await AssignRandomAttributesToVariantsAsync(db, logger, ct);

        logger?.LogInformation("Seeded sample catalog: {Count} products.", await db.Products.CountAsync(ct));
    }

    private static async Task SeedGamingLaptopAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics", ct);
        if (category == null)
        {
            category = new Category { Name = "Electronics", Slug = "electronics", IsActive = true };
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);
        }

        var productName = "Predator Helios Gaming Laptop";
        var productSlug = "predator-helios-gaming-laptop";
        
        var existingProduct = await db.Products.FirstOrDefaultAsync(p => p.Slug == productSlug, ct);
        if (existingProduct != null) return;

        var product = new Product
        {
            Name = productName,
            Slug = productSlug,
            Description = "Ultimate gaming performance with top-tier components. Customizable RAM, Storage, and GPU options to fit your needs.",
            BasePrice = 35000000,
            SoldCount = 45,
            CategoryId = category.Id,
            IsActive = true
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        db.ProductImages.Add(new ProductImage
        {
            ProductId = product.Id,
            ImageUrl = "https://images.unsplash.com/photo-1603302576837-37561b2e2302?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
            IsPrimary = true,
            AltText = productName
        });

        // Attributes
        var colors = new[] { "Obsidian Black", "Steel Gray" };
        var rams = new[] { "16GB", "32GB" };
        var storages = new[] { "512GB", "1TB", "2TB" };
        var gpus = new[] { "RTX 4070", "RTX 4080" };

        foreach (var color in colors)
        {
            foreach (var ram in rams)
            {
                foreach (var storage in storages)
                {
                    foreach (var gpu in gpus)
                    {
                        var sku = $"LAPTOP-{color.Substring(0, 3).ToUpper()}-{ram}-{storage}-{gpu.Replace(" ", "")}";
                        // Price increases with components
                        decimal price = 35000000;
                        if (ram == "32GB") price += 3000000;
                        if (storage == "1TB") price += 2000000;
                        else if (storage == "2TB") price += 4500000;
                        if (gpu == "RTX 4080") price += 8000000;

                        var variant = new ProductVariant
                        {
                            ProductId = product.Id,
                            Sku = sku,
                            Price = price,
                            StockQuantity = 10,
                            IsActive = true
                        };
                        db.ProductVariants.Add(variant);
                        await db.SaveChangesAsync(ct);

                        // Map attributes
                        var attrValues = new List<(string Name, string Value)>
                        {
                            ("Color", color),
                            ("RAM", ram),
                            ("Storage", storage),
                            ("GPU", gpu)
                        };

                        foreach (var av in attrValues)
                        {
                            var val = await db.AttributeValues.FirstOrDefaultAsync(v => v.Attribute!.Name == av.Name && v.Value == av.Value, ct);
                            if (val != null)
                            {
                                db.ProductVariantAttributes.Add(new ProductVariantAttribute { VariantId = variant.Id, AttributeValueId = val.Id });
                            }
                        }
                    }
                }
            }
        }

        logger?.LogInformation("Seeded multi-attribute gaming laptop: {Name}", productName);
    }

    private static async Task SeedAttributesAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var attributes = new Dictionary<string, string[]>
        {
            { "Color", new[] { "Red", "Blue", "Green", "Black", "White", "Silver", "Gold", "Pink", "Purple", "Yellow", "Orange", "Grey", "Brown", "Obsidian Black", "Steel Gray" } },
            { "Size", new[] { "S", "M", "L", "XL", "XXL", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45" } },
            { "Material", new[] { "Cotton", "Silk", "Leather", "Denim", "Polyester", "Nylon", "Wool", "Plastic", "Metal", "Wood", "Glass" } },
            { "Storage", new[] { "64GB", "128GB", "256GB", "512GB", "1TB", "2TB" } },
            { "RAM", new[] { "4GB", "8GB", "16GB", "32GB", "64GB" } },
            { "GPU", new[] { "RTX 4070", "RTX 4080", "RTX 4090" } }
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
            logger?.LogInformation("Seeded {AttrCount} attributes and {ValCount} attribute values.", createdAttr, createdVals);
        }
    }

    private static async Task AssignRandomAttributesToVariantsAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        // Only assign if we have variants and no assignments yet (to keep it idempotent-ish)
        if (await db.ProductVariantAttributes.AnyAsync(ct)) return;

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
            if (roll < 40 && colors.Count > 0) 
            {
                possibleValues.Add(colors[random.Next(colors.Count)]);
            }
            
            roll = random.Next(100);
            if (roll < 30 && sizes.Count > 0) 
            {
                possibleValues.Add(sizes[random.Next(sizes.Count)]);
            }

            roll = random.Next(100);
            if (roll < 20 && materials.Count > 0) 
            {
                possibleValues.Add(materials[random.Next(materials.Count)]);
            }

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
            logger?.LogInformation("Randomly assigned {Count} attributes to product variants.", assignments);
        }
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

    private static async Task SeedTestProductsAsync(ShopDbContext db, ILogger? logger, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Clothes", ct);
        if (category == null)
        {
            category = new Category { Name = "Clothes", Slug = "clothes", IsActive = true };
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);
        }

        var productName = "Premium Cotton T-Shirt";
        var productSlug = "premium-cotton-t-shirt";
        
        var existingProduct = await db.Products.FirstOrDefaultAsync(p => p.Slug == productSlug, ct);
        if (existingProduct != null) return;

        var product = new Product
        {
            Name = productName,
            Slug = productSlug,
            Description = "A high-quality 100% cotton t-shirt available in various colors and sizes. Comfortable, breathable, and perfect for any occasion.",
            BasePrice = 250000,
            SoldCount = 150,
            CategoryId = category.Id,
            IsActive = true
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        // Add an image
        db.ProductImages.Add(new ProductImage
        {
            ProductId = product.Id,
            ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
            IsPrimary = true,
            AltText = productName
        });

        // Define variants
        var colors = new[] { "Red", "Blue", "Black" };
        var sizes = new[] { "S", "M", "L" };

        foreach (var color in colors)
        {
            foreach (var size in sizes)
            {
                var sku = $"TSHIRT-{color.ToUpper()}-{size}";
                var variant = new ProductVariant
                {
                    ProductId = product.Id,
                    Sku = sku,
                    Price = 250000,
                    StockQuantity = 50,
                    IsActive = true
                };
                db.ProductVariants.Add(variant);
                await db.SaveChangesAsync(ct);

                // Add attributes to this variant
                var colorAttr = await db.AttributeValues.FirstOrDefaultAsync(v => v.Attribute!.Name == "Color" && v.Value == color, ct);
                var sizeAttr = await db.AttributeValues.FirstOrDefaultAsync(v => v.Attribute!.Name == "Size" && v.Value == size, ct);

                if (colorAttr != null)
                {
                    db.ProductVariantAttributes.Add(new ProductVariantAttribute { VariantId = variant.Id, AttributeValueId = colorAttr.Id });
                }
                if (sizeAttr != null)
                {
                    db.ProductVariantAttributes.Add(new ProductVariantAttribute { VariantId = variant.Id, AttributeValueId = sizeAttr.Id });
                }
            }
        }

        logger?.LogInformation("Seeded multi-variant test product: {Name}", productName);
    }
}
