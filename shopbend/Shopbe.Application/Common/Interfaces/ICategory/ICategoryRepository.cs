using CategoryEntity = Shopbe.Domain.Entities.Category.Category;

namespace Shopbe.Application.Common.Interfaces.ICategory;
public interface ICategoryRepository
{
    Task<IEnumerable<CategoryEntity>> GetAllCategoriesAsync();
    Task<IEnumerable<CategoryEntity>> GetRootCategoriesAsync();
    Task<IEnumerable<CategoryEntity>> GetChildrenAsync(Guid parentCategoryId);
    Task<CategoryEntity?> GetCategoryByIdAsync(Guid categoryId);
    Task<CategoryEntity?> GetCategoryBySlugAsync(string slug);
    Task<bool> ExistsBySlugAsync(string slug, Guid? excludingCategoryId = null);
    Task AddCategoryAsync(CategoryEntity category);
    Task UpdateCategoryAsync(CategoryEntity category);
    Task DeleteCategoryAsync(Guid categoryId);
}