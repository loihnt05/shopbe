using Shopbe.Domain.Entities.Payment;

namespace Shopbe.Application.Common.Interfaces.IPayment;

public interface IPaymentTransactionRepository
{
    Task<IReadOnlyList<PaymentTransaction>> ListByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default);
}

