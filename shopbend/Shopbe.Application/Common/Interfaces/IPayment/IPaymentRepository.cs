using Shopbe.Domain.Entities.Payment;

namespace Shopbe.Application.Common.Interfaces.IPayment;

public interface IPaymentRepository
{
    Task<Domain.Entities.Payment.Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Payment.Payment?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Payment.Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Payment.Payment>> ListByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Entities.Payment.Payment payment, CancellationToken cancellationToken = default);
}

