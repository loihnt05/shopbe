using Microsoft.EntityFrameworkCore;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Product;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.E2E.Tests.Infrastructure;

public static class DatabaseSeed
{
    public sealed record SeedResult(Guid CategoryId, Guid ProductId, Guid VariantId);

    public static async Task<SeedResult> SeedMinimalCatalogAsync(ShopDbContext db, CancellationToken ct = default)
    {
        // Ensure a category exists
        var category = await db.Categories.AsNoTracking().FirstOrDefaultAsync(ct);
        if (category is null)
        {
            category = new Category
            {
                Name = "E2E Category",
                Slug = "e2e-category",
                IsActive = true
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);
        }

        var product = new Product
        {
            Name = "E2E Product",
            Slug = $"e2e-product-{Guid.NewGuid():N}",
            Description = "Seeded for end-to-end tests",
            BasePrice = 100_000m,
            CategoryId = category.Id,
            IsActive = true
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Sku = $"E2E-SKU-{Guid.NewGuid():N}".Substring(0, 20),
            Price = 100_000m,
            StockQuantity = 10,
            IsActive = true
        };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(ct);

        return new SeedResult(category.Id, product.Id, variant.Id);
    }
}

