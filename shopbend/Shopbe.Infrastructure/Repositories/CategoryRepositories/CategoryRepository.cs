using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.ICategory;
using Shopbe.Domain.Entities.Category;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories;

public class CategoryRepository(ShopDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
    {
        return await context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == categoryId);
    }
    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
    {
        return await context.Categories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == null)
            .Include(c => c.SubCategories)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetChildrenAsync(Guid parentCategoryId)
    {
        return await context.Categories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .Include(c => c.SubCategories)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        var normalized = slug.Trim().ToLowerInvariant();
        return await context.Categories
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Slug.ToLower() == normalized);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, Guid? excludingCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        var normalized = slug.Trim().ToLowerInvariant();
        var query = context.Categories.AsQueryable();
        if (excludingCategoryId.HasValue)
        {
            query = query.Where(c => c.Id != excludingCategoryId.Value);
        }

        return await query.AnyAsync(c => c.Slug.ToLower() == normalized);
    }

    public async Task AddCategoryAsync(Category category)
    {
        await context.Categories.AddAsync(category);
    }
    public async Task UpdateCategoryAsync(Category category)
    {
        context.Categories.Update(category);
        await Task.CompletedTask;
    }
    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        var category = await context.Categories.FindAsync(categoryId);
        if (category != null)
        {
            context.Categories.Remove(category);
        }

        await Task.CompletedTask;
    }
    
}