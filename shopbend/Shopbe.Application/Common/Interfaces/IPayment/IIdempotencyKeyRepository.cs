using System.Text.Json;
using Shopbe.Domain.Entities.Payment;
using Shopbe.Domain.Enums;

namespace Shopbe.Application.Common.Interfaces.IPayment;

public interface IIdempotencyKeyRepository
{
    Task<IdempotencyKey?> GetAsync(string key, IdempotencyEntityType entityType, CancellationToken cancellationToken = default);
    Task AddAsync(IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Store a response json for an idempotency key (create if missing). Intended for "create" operations.
    /// </summary>
    Task UpsertResponseAsync(string key, IdempotencyEntityType entityType, Guid? entityId, JsonDocument response,
        DateTime expiresAtUtc, CancellationToken cancellationToken = default);
}

