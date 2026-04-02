using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.ICategory;
using Shopbe.Application.Common.Interfaces.IProduct;

namespace Shopbe.Application.Interfaces;
public interface IUnitOfWork
{
    IProductRepository Product { get; }
    ICategoryRepository Category { get; }
    IProductImageRepository ProductImage { get; }
    IProductVariantRepository ProductVariant { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}