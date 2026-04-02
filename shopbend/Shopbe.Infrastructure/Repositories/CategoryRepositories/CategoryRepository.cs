using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.ICategory;
using Shopbe.Application.Interfaces;
using Shopbe.Domain.Entities;
using Shopbe.Domain.Entities.Category;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories;

public class CategoryRepository(ShopDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
    {
        return await context.Categories.FindAsync(categoryId);
    }
    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await context.Categories.ToListAsync();
    }
    public async Task AddCategoryAsync(Category category)
    {
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
    }
    public async Task UpdateCategoryAsync(Category category)
    {
        context.Categories.Update(category);
        await context.SaveChangesAsync();
    }
    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        var category = await context.Categories.FindAsync(categoryId);
        if (category != null)
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        }
    }
    
}