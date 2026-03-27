namespace Shopbe.Domain.Entities.Payment;

public class PaymentTransaction : BaseEntity
{
    public Guid PaymentId { get; set; }
    public string ExternalTransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    // Navigation properties
    public Payment? Payment { get; set; }
}

public class PaymentLog : BaseEntity
{
    public Guid PaymentId { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }

    // Navigation properties
    public Payment? Payment { get; set; }
}

public class Refund : BaseEntity
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }

    // Navigation properties
    public Payment? Payment { get; set; }
}

public class IdempotencyKey : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string? RequestHash { get; set; }
    public string? ResponseBody { get; set; }
    public int? ResponseStatusCode { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

