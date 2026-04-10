using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IOrder;
using Shopbe.Domain.Entities.Order;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.OrderRepositories;

public class OrderRepository(ShopDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatusHistory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<Order?> GetByIdForUserAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatusHistory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);
    }

    public async Task<Order?> GetTrackedByIdForUserAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatusHistory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : pageSize;

        return await context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(o => o.OrderItems)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<long> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Orders.LongCountAsync(o => o.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await context.Orders.AddAsync(order, cancellationToken);
    }
}


