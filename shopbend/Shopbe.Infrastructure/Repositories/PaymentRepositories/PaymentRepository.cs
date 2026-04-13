using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IPayment;
using Shopbe.Domain.Entities.Payment;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.PaymentRepositories;

public sealed class PaymentRepository(ShopDbContext context) : IPaymentRepository
{
    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Payments
            .AsNoTracking()
            .Include(p => p.PaymentTransactions)
            .Include(p => p.Refunds)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Payment?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        => await context.Payments
            .AsNoTracking()
            .Include(p => p.Order)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id && p.Order != null && p.Order.UserId == userId, cancellationToken);

    public async Task<Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default)
        => await context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId, cancellationToken);

    public async Task<IReadOnlyList<Payment>> ListByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        => await context.Payments
            .AsNoTracking()
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        => await context.Payments.AddAsync(payment, cancellationToken);
}

