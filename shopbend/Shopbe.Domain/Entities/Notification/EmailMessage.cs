namespace Shopbe.Domain.Entities.Notification;

public class EmailMessage : BaseEntity
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }

    // Correlation / optional ownership
    public Guid? UserId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? PaymentId { get; set; }

    // Idempotency / deduping (optional; enforce uniqueness in EF config)
    public string? IdempotencyKey { get; set; }

    public EmailDeliveryStatus Status { get; set; } = EmailDeliveryStatus.Pending;
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; } = 5;
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? LastError { get; set; }
}

