using Shopbe.Application.Common.Interfaces.IBrand;
using Shopbe.Application.Common.Interfaces.ICategory;
using Shopbe.Application.Common.Interfaces.IProduct;
using Shopbe.Application.Common.Interfaces.IShoppingCart;
using Shopbe.Application.Common.Interfaces.IUser;

namespace Shopbe.Application.Common.Interfaces;
public interface IUnitOfWork
{
    
    // Users
    IUserRepository Users { get; }
    IUserAddressRepository UserAddresses { get; }
    
    // Categories
    ICategoryRepository Category { get; }

    // Brands
    IBrandRepository Brand { get; }
    
    // Products
    IProductRepository Product { get; }
    IProductImageRepository ProductImage { get; }
    IProductVariantRepository ProductVariant { get; }
    IProductVariantAttributeRepository ProductVariantAttribute { get; }
    IProductAttributeRepository ProductAttribute { get; }
    IAttributeValueRepository AttributeValue { get; }

    // Shopping cart
    ICartRepository Cart { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}