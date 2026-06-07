using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopbe.Domain.Entities.Notification;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Services.Email;

public sealed class EmailRecoveryJob(
    ShopDbContext db,
    IBackgroundJobClient backgroundJobs,
    ILogger<EmailRecoveryJob> logger)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(int batchSize = 100, int retryDelayMinutes = 5, CancellationToken cancellationToken = default)
    {
        batchSize = Math.Clamp(batchSize, 1, 500);
        retryDelayMinutes = Math.Clamp(retryDelayMinutes, 1, 1440);

        var retryBefore = DateTime.UtcNow.AddMinutes(-retryDelayMinutes);

        var messages = await db.EmailMessages
            .Where(m => (m.Status == EmailDeliveryStatus.Pending || m.Status == EmailDeliveryStatus.Failed)
                        && m.AttemptCount < m.MaxAttempts
                        && (m.LastAttemptAt == null || m.LastAttemptAt < retryBefore))
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var message in messages)
        {
            // Stamp recovery pickup time so the recurring job does not immediately re-enqueue the same message.
            message.LastAttemptAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var message in messages)
        {
            backgroundJobs.Enqueue<EmailProcessorJob>(job => job.ProcessAsync(message.Id, CancellationToken.None));
        }

        logger.LogInformation("Re-enqueued {Count} email messages for recovery processing.", messages.Count);
    }
}
