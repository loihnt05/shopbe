namespace Shopbe.Application.Common.Interfaces.IOrder;

public interface IOrderRepository
{
    Task<Shopbe.Domain.Entities.Order.Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Shopbe.Domain.Entities.Order.Order?> GetByIdForUserAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);
    Task<Shopbe.Domain.Entities.Order.Order?> GetTrackedByIdForUserAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shopbe.Domain.Entities.Order.Order>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<long> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Shopbe.Domain.Entities.Order.Order order, CancellationToken cancellationToken = default);
}



