namespace Shopbe.Domain.Entities.Notification;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }

    // Navigation properties
    public User.User? User { get; set; }
    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}
