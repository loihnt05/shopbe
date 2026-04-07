using System.Text.Json;
using Shopbe.Domain.Enums;

namespace Shopbe.Domain.Entities.Payment;

public class IdempotencyKey : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public IdempotencyEntityType EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public JsonDocument? Response { get; set; }
    public DateTime ExpiresAt { get; set; }
}

