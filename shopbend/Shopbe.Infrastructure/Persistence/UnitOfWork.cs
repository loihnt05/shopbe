using Microsoft.EntityFrameworkCore.Storage;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.IBrand;
using Shopbe.Application.Common.Interfaces.ICategory;
using Shopbe.Application.Common.Interfaces.IOrder;
using Shopbe.Application.Common.Interfaces.IProduct;
using Shopbe.Application.Common.Interfaces.IShipping;
using Shopbe.Application.Common.Interfaces.IShoppingCart;
using Shopbe.Application.Common.Interfaces.IUser;
using Shopbe.Infrastructure.Repositories;
using Shopbe.Infrastructure.Repositories.BrandRepositories;
using Shopbe.Infrastructure.Repositories.OrderRepositories;
using Shopbe.Infrastructure.Repositories.ProductRepositories;
using Shopbe.Infrastructure.Repositories.ShippingRepositories;
using Shopbe.Infrastructure.Repositories.ShoppingCartRepositories;
using Shopbe.Infrastructure.Repositories.UserRepository;

namespace Shopbe.Infrastructure.Persistence;

public class UnitOfWork(ShopDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    // Users
    public IUserRepository Users { get; } = new UserRepository(context);
    public IUserAddressRepository UserAddresses { get; } = new UserAddressRepository(context);
    // Categories
    public ICategoryRepository Category { get; } = new CategoryRepository(context);
    // Brands
    public IBrandRepository Brand { get; } = new BrandRepository(context);
    // Products
    public IProductRepository Product { get; } = new ProductRepository(context);
    public IProductVariantRepository ProductVariant { get; } = new ProductVariantRepository(context);
    public IProductVariantAttributeRepository ProductVariantAttribute { get; } = new ProductVariantAttributeRepository(context);
    public IProductImageRepository ProductImage { get; } = new ProductImageRepository(context);
    public IProductAttributeRepository ProductAttribute { get; } = new ProductAttributeRepository(context);
    public IAttributeValueRepository AttributeValue { get; } = new AttributeValueRepository(context);

    // Shopping cart
    public ICartRepository Cart { get; } = new CartRepository(context);

    // Orders
    public IOrderRepository Orders { get; } = new OrderRepository(context);
    public ICouponRepository Coupons { get; } = new CouponRepository(context);

    // Shipping
    public IShippingZoneRepository ShippingZones { get; } = new ShippingZoneRepository(context);
    public IShippingZoneDistrictRepository ShippingZoneDistricts { get; } = new ShippingZoneDistrictRepository(context);
    public IShipmentRepository Shipments { get; } = new ShipmentRepository(context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }
}