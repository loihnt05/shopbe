using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopbe.Application.Product.Products.Dtos;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Infrastructure.Persistence;

public static class EscuelaSeeder
{
    private const decimal VndExchangeRate = 25000m;

    public static async Task SeedAsync(ShopDbContext db, ILogger? logger = null, CancellationToken ct = default)
    {
        logger?.LogInformation("Fetching products from EscuelaJS (Platzi Fake Store)...");
        using var http = new HttpClient();
        
        List<EscuelaProductDto>? products;
        try
        {
            // Fetch 100 products
            var url = "https://api.escuelajs.co/api/v1/products?offset=0&limit=100";
            products = await http.GetFromJsonAsync<List<EscuelaProductDto>>(url, ct);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to fetch products from EscuelaJS.");
            return;
        }

        if (products == null || products.Count == 0)
        {
            logger?.LogWarning("No products found from EscuelaJS.");
            return;
        }

        logger?.LogInformation("Seeding {Count} products from EscuelaJS...", products.Count);

        var usedCategorySlugs = new HashSet<string>(
            await db.Categories.Select(c => c.Slug).ToListAsync(ct), StringComparer.OrdinalIgnoreCase);
        var usedProductSlugs = new HashSet<string>(
            await db.Products.Select(p => p.Slug).ToListAsync(ct), StringComparer.OrdinalIgnoreCase);
        var usedSkus = new HashSet<string>(
            await db.ProductVariants.Select(v => v.Sku).ToListAsync(ct), StringComparer.OrdinalIgnoreCase);

        var categoryMap = new Dictionary<int, Category>();

        // 1. Prepare Categories
        var uniqueCategories = products
            .Select(p => p.Category)
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToList();

        foreach (var catDto in uniqueCategories)
        {
            var slug = Slugify(catDto.Name);
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug, ct);
            if (category == null)
            {
                category = new Category
                {
                    Name = ToTitleCase(catDto.Name),
                    Slug = UniqueSlug(slug, usedCategorySlugs),
                    IsActive = true,
                    SortOrder = 0
                };
                db.Categories.Add(category);
                await db.SaveChangesAsync(ct);
            }
            categoryMap[catDto.Id] = category;
        }

        // 2. Seed Products
        foreach (var ep in products)
        {
            var slugBase = Slugify(ep.Title);
            if (usedProductSlugs.Contains(slugBase)) continue;

            if (!categoryMap.TryGetValue(ep.Category.Id, out var category))
            {
                // Fallback or skip if category wasn't created
                continue;
            }

            // Realistic sold count
            var soldCount = RandomNumberGenerator.GetInt32(50, 1000);

            var product = new Product
            {
                Name = ep.Title,
                Slug = UniqueSlug(slugBase, usedProductSlugs),
                Description = ep.Description,
                BasePrice = ep.Price * VndExchangeRate,
                SoldCount = soldCount,
                CategoryId = category.Id,
                IsActive = true
            };
            db.Products.Add(product);
            await db.SaveChangesAsync(ct);

            // Images
            if (ep.Images != null && ep.Images.Count > 0)
            {
                for (int i = 0; i < ep.Images.Count; i++)
                {
                    // Platzi sometimes returns images as JSON strings or with escaped quotes
                    var imgUrl = ep.Images[i].Trim('[', ']', '"');
                    
                    db.ProductImages.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = imgUrl,
                        AltText = product.Name,
                        IsPrimary = i == 0,
                        SortOrder = i
                    });
                }
            }

            // Variants
            var skuBase = $"ESC_{product.Slug.Replace('-', '_').ToUpperInvariant()}";
            db.ProductVariants.Add(new ProductVariant
            {
                ProductId = product.Id,
                Sku = UniqueSku(skuBase, usedSkus),
                Price = product.BasePrice,
                StockQuantity = RandomNumberGenerator.GetInt32(10, 200),
                IsActive = true
            });

            await db.SaveChangesAsync(ct);
        }

        logger?.LogInformation("EscuelaJS seeding complete. Total products: {Count}", await db.Products.CountAsync(ct));
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
        var original = slug;
        while (!used.Add(slug))
        {
            slug = $"{original}-{i++}";
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

    private static string ToTitleCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToUpper(str[0]) + str[1..].Replace("-", " ");
    }
}
