using Microsoft.EntityFrameworkCore.Storage;
using Shopbe.Application.Common.Interfaces.ICategory;
using Shopbe.Application.Common.Interfaces.IProduct;
using Shopbe.Application.Common.Interfaces.IUser;
using Shopbe.Application.Interfaces;
using Shopbe.Infrastructure.Repositories;
using Shopbe.Infrastructure.Repositories.ProductRepositories;
using Shopbe.Infrastructure.Repositories.UserRepository;

namespace Shopbe.Infrastructure.Persistence;

public class UnitOfWork(ShopDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    // Users
    public IUserRepository User { get; } = new UserRepository(context);
    public IUserAddressRepository UserAddress { get; } = new UserAddressRepository(context);
    // Categories
    public ICategoryRepository Category { get; } = new CategoryRepository(context);
    // Products
    public IProductRepository Product { get; } = new ProductRepository(context);
    public IProductVariantRepository ProductVariant { get; } = new ProductVariantRepository(context);
    public IProductImageRepository ProductImage { get; } = new ProductImageRepository(context);

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