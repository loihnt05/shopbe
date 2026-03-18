namespace Shopbe.Application.Interfaces;
public interface IUnitOfWork
{
    IProductRepository Product { get; }
    ICategoryRepository Category { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}