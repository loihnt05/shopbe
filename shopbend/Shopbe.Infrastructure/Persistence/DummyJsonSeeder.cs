using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopbe.Application.Product.Products.Dtos;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Infrastructure.Persistence;

public static class DummyJsonSeeder
{
    private const decimal VndExchangeRate = 25000m;

    public static async Task SeedAsync(ShopDbContext db, ILogger? logger = null, int maxCount = int.MaxValue, CancellationToken ct = default)
    {
        var adminUser = await db.Users.FirstOrDefaultAsync(u => u.Role == Domain.Enums.UserRole.Admin, ct);
        if (adminUser == null) return;

        logger?.LogInformation("Fetching products from DummyJSON...");
        using var http = new HttpClient();
        
        // Fetch products from DummyJSON.
        DummyResponseDto? response;
        try
        {
            var url = $"https://dummyjson.com/products?limit={maxCount}";
            response = await http.GetFromJsonAsync<DummyResponseDto>(url, ct);
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

        var productsToSeed = response.Products.Take(maxCount).ToList();
        logger?.LogInformation("Seeding {Count} products from DummyJSON...", productsToSeed.Count);

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
        var uniqueCategories = productsToSeed.Select(p => p.Category).Distinct(StringComparer.OrdinalIgnoreCase);
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
        var uniqueBrands = productsToSeed.Select(p => p.Brand).Where(b => !string.IsNullOrEmpty(b)).Distinct(StringComparer.OrdinalIgnoreCase);
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
        foreach (var dp in productsToSeed)
        {
            var slugBase = Slugify(dp.Title);
            if (usedProductSlugs.Contains(slugBase)) continue;

            var category = categoryMap[dp.Category];
            var brand = !string.IsNullOrEmpty(dp.Brand) && brandMap.TryGetValue(dp.Brand, out var b) ? b : null;

            // Realistic sold count: higher rating generally means more sales.
            var baseSold = (int)(dp.Rating * 100);
            var soldCount = baseSold + RandomNumberGenerator.GetInt32(0, 500);

            var product = new Product
            {
                Name = dp.Title,
                Slug = UniqueSlug(slugBase, usedProductSlugs),
                Description = dp.Description,
                BasePrice = dp.Price * VndExchangeRate,
                SoldCount = soldCount,
                CategoryId = category.Id,
                BrandId = brand?.Id,
                IsActive = true,
                SellerId = adminUser.Id
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
                        ImageUrl = NormalizeImageUrl(dp.Images[i]),
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
                    ImageUrl = NormalizeImageUrl(dp.Thumbnail),
                    AltText = product.Name,
                    IsPrimary = true,
                    SortOrder = 0
                });
            }

            // Variants
            var skuBase = LimitLength(!string.IsNullOrEmpty(dp.Sku) ? dp.Sku : $"{product.Slug.Replace('-', '_').ToUpperInvariant()}_1", 100);
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
        var sku = LimitLength(skuBase, 100);
        var i = 1;
        while (!used.Add(sku))
        {
            var suffix = $"_{i++}";
            sku = $"{LimitLength(skuBase, 100 - suffix.Length)}{suffix}";
        }
        return sku;
    }

    private static string NormalizeImageUrl(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length <= 500) return trimmed;

        var queryIndex = trimmed.IndexOf('?');
        if (queryIndex > 0)
        {
            trimmed = trimmed[..queryIndex];
        }

        return LimitLength(trimmed, 500);
    }

    private static string LimitLength(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string ToTitleCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToUpper(str[0]) + str[1..].Replace("-", " ");
    }
}
