using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Services;

public sealed class UserBehaviorCleanupJob(ShopDbContext db, ILogger<UserBehaviorCleanupJob> logger)
{
    /// <summary>
    /// Delete expired user behaviors in batches.
    /// Safe to run frequently (idempotent).
    /// </summary>
    public async Task<int> RunAsync(int batchSize = 20_000, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var totalDeleted = 0;

        while (true)
        {
            // Select just ids to avoid loading full entities.
            var ids = await db.UserBehaviors
                .AsNoTracking()
                .Where(x => x.ExpiresAt < now)
                .OrderBy(x => x.ExpiresAt)
                .Select(x => x.Id)
                .Take(batchSize)
                .ToListAsync(ct);

            if (ids.Count == 0)
            {
                break;
            }

            var deleted = await db.UserBehaviors
                .Where(x => ids.Contains(x.Id))
                .ExecuteDeleteAsync(ct);

            totalDeleted += deleted;

            logger.LogInformation("UserBehaviorCleanupJob deleted {Deleted} expired user behaviors (TotalDeleted={TotalDeleted}).", deleted, totalDeleted);

            // If we deleted fewer than the batch, we are likely done.
            if (deleted < batchSize)
            {
                break;
            }
        }

        return totalDeleted;
    }
}

