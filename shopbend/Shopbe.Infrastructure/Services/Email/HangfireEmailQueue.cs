using Hangfire;
using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.Notifications;
using Shopbe.Domain.Entities.Notification;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Services.Email;

public sealed class HangfireEmailQueue(ShopDbContext db, IBackgroundJobClient backgroundJobs) : IEmailQueue
{
    public async Task<Guid> EnqueueAsync(
        string to,
        string subject,
        string bodyHtml,
        string? bodyText = null,
        Guid? userId = null,
        Guid? orderId = null,
        Guid? paymentId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await db.EmailMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);

            if (existing is not null)
            {
                return existing.Id;
            }
        }

        var message = new EmailMessage
        {
            Id = Guid.NewGuid(),
            To = to,
            Subject = subject,
            BodyHtml = bodyHtml,
            BodyText = bodyText,
            UserId = userId,
            OrderId = orderId,
            PaymentId = paymentId,
            IdempotencyKey = idempotencyKey,
            Status = EmailDeliveryStatus.Pending,
            AttemptCount = 0
        };

        db.EmailMessages.Add(message);
        await db.SaveChangesAsync(cancellationToken);

        backgroundJobs.Enqueue<EmailProcessorJob>(job => job.ProcessAsync(message.Id, CancellationToken.None));

        return message.Id;
    }
}

