using Microsoft.EntityFrameworkCore.Storage;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Interfaces;
using Shopbe.Application.Interfaces.Repositories;
using Shopbe.Infrastructure.Repositories;

namespace Shopbe.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ShopDbContext _context;
    private IDbContextTransaction? _transaction;
    public IProductRepository Product { get; }
    public ICategoryRepository Category { get; }
    public IProductVariantRepository ProductVariant { get; }
    public IProductImageRepository ProductImage { get; }

    public UnitOfWork(ShopDbContext context)
    {
        _context = context;
        Product = new ProductRepository(context);
        Category = new CategoryRepository(context);
        ProductImage = new ProductImageRepository(context);
        ProductVariant = new ProductVariantRepository(context);
    }
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
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
        _context.Dispose();
    }
}