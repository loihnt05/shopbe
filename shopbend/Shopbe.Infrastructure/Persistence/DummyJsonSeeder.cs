using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Infrastructure.Persistence;

public static class DummyJsonSeeder
{
    private const decimal VndExchangeRate = 25000m;

    public static async Task SeedAsync(ShopDbContext db, ILogger? logger = null, CancellationToken ct = default)
    {
        logger?.LogInformation("Fetching products from DummyJSON...");
        using var http = new HttpClient();
        
        // Fetch all products from DummyJSON.
        DummyResponse? response;
        try
        {
            var url = "https://dummyjson.com/products?limit=0";
            response = await http.GetFromJsonAsync<DummyResponse>(url, ct);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to fetch products from DummyJSON.");
            return;
        }

        if (response == null || response.Products.Count == 0)
        {
            logger?.LogWarning("No products found from DummyJSON.");
            return;
        }

        logger?.LogInformation("Seeding {Count} products from DummyJSON (total available: {Total})...", response.Products.Count, response.Total);

        var usedCategorySlugs = new HashSet<string>(
            await db.Categories.Select(c => c.Slug).ToListAsync(ct), StringComparer.OrdinalIgnoreCase);
        var usedBrandSlugs = new HashSet<string>(
            await db.Brands.Select(b => b.Slug).ToListAsync(ct), StringComparer.OrdinalIgnoreCase);
        var usedProductSlugs = new HashSet<string>(
            await db.Products.Select(p => p.Slug).ToListAsync(ct), StringComparer.OrdinalIgnoreCase);
        var usedSkus = new HashSet<string>(
            await db.ProductVariants.Select(v => v.Sku).ToListAsync(ct), StringComparer.OrdinalIgnoreCase);

        var categoryMap = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
        var brandMap = new Dictionary<string, Brand>(StringComparer.OrdinalIgnoreCase);

        // 1. Prepare Categories
        var uniqueCategories = response.Products.Select(p => p.Category).Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var catName in uniqueCategories)
        {
            var slug = Slugify(catName);
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug, ct);
            if (category == null)
            {
                category = new Category
                {
                    Name = ToTitleCase(catName),
                    Slug = UniqueSlug(slug, usedCategorySlugs),
                    IsActive = true,
                    SortOrder = 0
                };
                db.Categories.Add(category);
                await db.SaveChangesAsync(ct);
            }
            categoryMap[catName] = category;
        }

        // 2. Prepare Brands
        var uniqueBrands = response.Products.Select(p => p.Brand).Where(b => !string.IsNullOrEmpty(b)).Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var brandName in uniqueBrands!)
        {
            var slug = Slugify(brandName!);
            var brand = await db.Brands.FirstOrDefaultAsync(b => b.Slug == slug, ct);
            if (brand == null)
            {
                brand = new Brand
                {
                    Name = brandName!,
                    Slug = UniqueSlug(slug, usedBrandSlugs),
                    IsActive = true
                };
                db.Brands.Add(brand);
                await db.SaveChangesAsync(ct);
            }
            brandMap[brandName!] = brand;
        }

        // 3. Seed Products
        foreach (var dp in response.Products)
        {
            var slugBase = Slugify(dp.Title);
            if (usedProductSlugs.Contains(slugBase)) continue;

            var category = categoryMap[dp.Category];
            var brand = !string.IsNullOrEmpty(dp.Brand) && brandMap.TryGetValue(dp.Brand, out var b) ? b : null;

            var product = new Product
            {
                Name = dp.Title,
                Slug = UniqueSlug(slugBase, usedProductSlugs),
                Description = dp.Description,
                BasePrice = dp.Price * VndExchangeRate,
                CategoryId = category.Id,
                BrandId = brand?.Id,
                IsActive = true
            };
            db.Products.Add(product);
            await db.SaveChangesAsync(ct);

            // Images
            if (dp.Images != null && dp.Images.Count > 0)
            {
                for (int i = 0; i < dp.Images.Count; i++)
                {
                    db.ProductImages.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = dp.Images[i],
                        AltText = product.Name,
                        IsPrimary = i == 0,
                        SortOrder = i
                    });
                }
            }
            else if (!string.IsNullOrEmpty(dp.Thumbnail))
            {
                db.ProductImages.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = dp.Thumbnail,
                    AltText = product.Name,
                    IsPrimary = true,
                    SortOrder = 0
                });
            }

            // Variants
            var skuBase = !string.IsNullOrEmpty(dp.Sku) ? dp.Sku : $"{product.Slug.Replace('-', '_').ToUpperInvariant()}_1";
            db.ProductVariants.Add(new ProductVariant
            {
                ProductId = product.Id,
                Sku = UniqueSku(skuBase, usedSkus),
                Price = product.BasePrice,
                StockQuantity = dp.Stock,
                IsActive = true
            });

            await db.SaveChangesAsync(ct);
        }

        logger?.LogInformation("DummyJSON seeding complete. Total products: {Count}", await db.Products.CountAsync(ct));
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

    private class DummyProduct
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Brand { get; set; }
        public string? Sku { get; set; }
        public int Stock { get; set; }
        public List<string>? Images { get; set; }
        public string? Thumbnail { get; set; }
    }

    private class DummyResponse
    {
        public List<DummyProduct> Products { get; set; } = new();
        public int Total { get; set; }
    }
}
