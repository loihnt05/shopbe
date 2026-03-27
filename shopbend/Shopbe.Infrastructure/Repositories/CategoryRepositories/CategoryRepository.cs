using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Interfaces;
using Shopbe.Domain.Entities;
using Shopbe.Domain.Entities.Category;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ShopDbContext _context;

    public CategoryRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
    {
        return await _context.Categories.FindAsync(categoryId);
    }
    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories.ToListAsync();
    }
    public async Task AddCategoryAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateCategoryAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        var category = await _context.Categories.FindAsync(categoryId);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
    
}