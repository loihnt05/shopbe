using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IPayment;
using Shopbe.Domain.Entities.Payment;
using Shopbe.Domain.Enums;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.PaymentRepositories;

public sealed class IdempotencyKeyRepository(ShopDbContext context) : IIdempotencyKeyRepository
{
    public async Task<IdempotencyKey?> GetAsync(string key, IdempotencyEntityType entityType,
        CancellationToken cancellationToken = default)
        => await context.IdempotencyKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Key == key && i.EntityType == entityType && i.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

    public async Task AddAsync(IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default)
        => await context.IdempotencyKeys.AddAsync(idempotencyKey, cancellationToken);

    public async Task UpsertResponseAsync(string key, IdempotencyEntityType entityType, Guid? entityId,
        JsonDocument response, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
    {
        var existing = await context.IdempotencyKeys
            .FirstOrDefaultAsync(i => i.Key == key && i.EntityType == entityType, cancellationToken);

        if (existing is null)
        {
            existing = new IdempotencyKey
            {
                Key = key,
                EntityType = entityType,
                EntityId = entityId,
                Response = response,
                ExpiresAt = expiresAtUtc
            };
            context.IdempotencyKeys.Add(existing);
        }
        else
        {
            existing.EntityId = entityId;
            existing.Response = response;
            existing.ExpiresAt = expiresAtUtc;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

