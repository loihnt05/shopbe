using BrandEntity = Shopbe.Domain.Entities.Product.Brand;

namespace Shopbe.Application.Common.Interfaces.IBrand;

public interface IBrandRepository
{
    Task<IEnumerable<BrandEntity>> GetAllBrandsAsync();
    Task<BrandEntity?> GetBrandByIdAsync(Guid brandId);
    Task<BrandEntity?> GetBrandBySlugAsync(string slug);
    Task<bool> ExistsBySlugAsync(string slug, Guid? excludingBrandId = null);
    Task AddBrandAsync(BrandEntity brand);
    Task UpdateBrandAsync(BrandEntity brand);
    Task DeleteBrandAsync(Guid brandId);
}

