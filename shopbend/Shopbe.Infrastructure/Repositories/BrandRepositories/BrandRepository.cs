using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IBrand;
using Shopbe.Domain.Entities.Product;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.BrandRepositories;

public class BrandRepository(ShopDbContext context) : IBrandRepository
{
    public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
    {
        return await context.Brands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<Brand?> GetBrandByIdAsync(Guid brandId)
    {
        return await context.Brands
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.Id == brandId);
    }

    public async Task<Brand?> GetBrandBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        var normalized = slug.Trim().ToLowerInvariant();
        return await context.Brands
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Slug.ToLower() == normalized);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, Guid? excludingBrandId = null)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        var normalized = slug.Trim().ToLowerInvariant();
        var query = context.Brands.AsQueryable();

        if (excludingBrandId.HasValue)
        {
            query = query.Where(b => b.Id != excludingBrandId.Value);
        }

        return await query.AnyAsync(b => b.Slug.ToLower() == normalized);
    }

    public async Task AddBrandAsync(Brand brand)
    {
        await context.Brands.AddAsync(brand);
    }

    public async Task UpdateBrandAsync(Brand brand)
    {
        context.Brands.Update(brand);
        await Task.CompletedTask;
    }

    public async Task DeleteBrandAsync(Guid brandId)
    {
        var brand = await context.Brands.FindAsync(brandId);
        if (brand != null)
        {
            context.Brands.Remove(brand);
        }

        await Task.CompletedTask;
    }
}

