using CategoryEntity = Shopbe.Domain.Entities.Category.Category;

namespace Shopbe.Application.Interfaces;
public interface ICategoryRepository
{
    Task<IEnumerable<CategoryEntity>> GetAllCategoriesAsync();
    Task<CategoryEntity?> GetCategoryByIdAsync(Guid categoryId);
    Task AddCategoryAsync(CategoryEntity category);
    Task UpdateCategoryAsync(CategoryEntity category);
    Task DeleteCategoryAsync(Guid categoryId);
}