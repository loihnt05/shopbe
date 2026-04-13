using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IPayment;
using Shopbe.Domain.Entities.Payment;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.PaymentRepositories;

public sealed class RefundRepository(ShopDbContext context) : IRefundRepository
{
    public async Task<Refund?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Refunds
            .AsNoTracking()
            .Include(r => r.Payment)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Refund>> ListByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
        => await context.Refunds
            .AsNoTracking()
            .Where(r => r.PaymentId == paymentId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Refund refund, CancellationToken cancellationToken = default)
        => await context.Refunds.AddAsync(refund, cancellationToken);
}

