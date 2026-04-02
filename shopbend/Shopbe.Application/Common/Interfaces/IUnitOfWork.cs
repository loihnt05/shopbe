using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.ICategory;
using Shopbe.Application.Common.Interfaces.IProduct;
using Shopbe.Application.Common.Interfaces.IUser;

namespace Shopbe.Application.Interfaces;
public interface IUnitOfWork
{
    
    // Users
    IUserRepository Users { get; }
    IUserAddressRepository UserAddresses { get; }
    
    // Categories
    ICategoryRepository Category { get; }
    
    // Products
    IProductRepository Product { get; }
    IProductImageRepository ProductImage { get; }
    IProductVariantRepository ProductVariant { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}