namespace Shopbe.Domain.Entities.Notification;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Type { get; set; } = "System";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public User.User? User { get; set; }
    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}
