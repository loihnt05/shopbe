using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IPayment;
using Shopbe.Domain.Entities.Payment;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.PaymentRepositories;

public sealed class PaymentTransactionRepository(ShopDbContext context) : IPaymentTransactionRepository
{
    public async Task<IReadOnlyList<PaymentTransaction>> ListByPaymentIdAsync(Guid paymentId,
        CancellationToken cancellationToken = default)
        => await context.PaymentTransactions
            .AsNoTracking()
            .Where(t => t.PaymentId == paymentId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
        => await context.PaymentTransactions.AddAsync(transaction, cancellationToken);
}

