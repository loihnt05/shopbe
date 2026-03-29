using Microsoft.EntityFrameworkCore.Storage;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Interfaces;
using Shopbe.Application.Interfaces.Repositories;
using Shopbe.Infrastructure.Repositories;
using Shopbe.Infrastructure.Repositories.ProductRepositories;

namespace Shopbe.Infrastructure.Persistence;

public class UnitOfWork(ShopDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    public IProductRepository Product { get; } = new ProductRepository(context);
    public ICategoryRepository Category { get; } = new CategoryRepository(context);
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