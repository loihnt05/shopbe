namespace Shopbe.Domain.Entities.Notification;

public class NotificationLog : BaseEntity
{
    public Guid NotificationId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }

    // Navigation Properties
    public Notification? Notification { get; set; }
}

