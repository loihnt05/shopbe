using Microsoft.EntityFrameworkCore;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Product;
using Shopbe.Domain.Entities.User;
using Shopbe.Domain.Enums;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.E2E.Tests.Infrastructure;

public static class DatabaseSeed
{
    public sealed record SeedResult(Guid CategoryId, Guid ProductId, Guid VariantId);

    public static async Task<SeedResult> SeedMinimalCatalogAsync(ShopDbContext db, CancellationToken ct = default)
    {
        // Ensure an admin user exists for SellerId FK
        var adminUser = await db.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin, ct);
        if (adminUser is null)
        {
            adminUser = new User
            {
                KeycloakId = "e2e-admin",
                Email = "e2e-admin@test.com",
                FullName = "E2E Admin",
                Role = UserRole.Admin,
                Status = UserStatus.Active
            };
            db.Users.Add(adminUser);
            await db.SaveChangesAsync(ct);
        }

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
            IsActive = true,
            SellerId = adminUser.Id
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

