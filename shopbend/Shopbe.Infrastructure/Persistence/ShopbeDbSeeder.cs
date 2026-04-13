using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Infrastructure.Persistence;

/// <summary>
/// Simple development seeder to create a few categories/products/variants so you can test the buying flow manually.
/// Safe to run multiple times (idempotent by slug/sku).
/// </summary>
public static class ShopbeDbSeeder
{
    public static async Task SeedAsync(ShopDbContext db, ILogger? logger = null, CancellationToken ct = default)
    {
        // Only seed if we don't have any products yet.
        if (await db.Products.AsNoTracking().AnyAsync(ct))
        {
            logger?.LogInformation("Seeder skipped: products already exist.");
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
}

